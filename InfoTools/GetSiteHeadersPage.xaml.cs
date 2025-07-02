using System.Net.Http;
using System.Windows;
using System.Windows.Controls;

namespace InfoTools
{
    /// <summary>
    /// Interaction logic for GetSiteHeadersPage.xaml
    /// </summary>
    public partial class GetSiteHeadersPage : Page
    {
        public GetSiteHeadersPage()
        {
            InitializeComponent();
            UrlTextBox.TextChanged += (s, e) => CheckHeadersButton.IsEnabled = IsValidUrl(UrlTextBox.Text);
        }

        private static bool IsValidUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                return false;

            if (string.IsNullOrWhiteSpace(uri.Scheme) || string.IsNullOrWhiteSpace(uri.Host))
                return false;

            // Only allow valid URL characters (RFC 3986)
            var invalidCharPattern = @"[ <>""{}|\\^`[\]\x00-\x1F\x7F]";
            if (System.Text.RegularExpressions.Regex.IsMatch(url, invalidCharPattern))
                return false;

            // Accept localhost
            if (uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
                return true;

            // Accept valid IP addresses
            if (System.Net.IPAddress.TryParse(uri.Host, out _))
                return true;

            // Host must contain a dot and a valid TLD (at least 2 letters)
            var hostParts = uri.Host.Split('.');
            if (hostParts.Length >= 2)
            {
                var tld = hostParts[^1];
                if (tld.Length >= 2 && tld.All(char.IsLetter))
                    return true;
            }

            return false;
        }

        private async void CheckHeadersButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                HeadersPanel.Children.Clear(); // Clear previous data

                using var client = new HttpClient();
                var response = await client.GetAsync(UrlTextBox.Text);
                response.EnsureSuccessStatusCode();

                var headers = response.Headers;
                foreach (var header in headers)
                {
                    var values = string.Join(", ", header.Value);
                    var headerLabel = new Label { Content = $"{header.Key}: {values}", FontSize = 14, Margin = new Thickness(0, 5, 0, 0)};
                    HeadersPanel.Children.Add(headerLabel);
                }
            }
            catch (HttpRequestException ex)
            {
                var errorLabel = new Label { Content = $"Error: {ex.Message}", Foreground = System.Windows.Media.Brushes.Red, FontSize = 14 };
                HeadersPanel.Children.Add(errorLabel);
            }
        }
    }
}