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

        private bool IsValidUrl(string url)
        {
            // Simple check: not empty and contains "http://" or "https://"
            return !string.IsNullOrWhiteSpace(url) && (url.Contains("http://") || url.Contains("https://"));
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
                    var headerLabel = new Label { Content = $"{header.Key}: {header.Value}", FontSize = 14, Margin = "0,5" };
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