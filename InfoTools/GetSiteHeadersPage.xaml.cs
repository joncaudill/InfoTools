using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.IO;
using System;

namespace InfoTools
{
    /// <summary>
    /// Interaction logic for GetSiteHeadersPage.xaml
    /// </summary>
    public partial class GetSiteHeadersPage : Page
    {
        private readonly FaviconService _faviconService = new();

        /// <summary>
        /// Initializes a new instance of the GetSiteHeadersPage class.
        /// </summary>
        public GetSiteHeadersPage()
        {
            InitializeComponent();
            UrlTextBox.TextChanged += (s, e) => CheckHeadersButton.IsEnabled = IsValidUrl(UrlTextBox.Text);

            // Load favicon database
            _faviconService.LoadFaviconDatabase();
        }

        /// <summary>
        /// Validates if the provided URL string is a valid URL format.
        /// </summary>
        /// <param name="url">The URL string to validate.</param>
        /// <returns>True if the URL is valid, false otherwise.</returns>
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

        /// <summary>
        /// Handles the click event for the Check Headers button, retrieving site headers and favicon.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The routed event arguments.</param>
        private async void CheckHeadersButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                HeadersPanel.Children.Clear(); // Clear previous data
                FaviconSection.Visibility = Visibility.Collapsed;
                FaviconImage.Source = null;
                FaviconResultText.Text = "";

                using var client = new HttpClient();
                var response = await client.GetAsync(UrlTextBox.Text);
                response.EnsureSuccessStatusCode();

                // Display headers
                var headers = response.Headers;
                foreach (var header in headers)
                {
                    var values = string.Join(", ", header.Value);
                    var headerLabel = new Label { Content = $"{header.Key}: {values}", FontSize = 14, Margin = new Thickness(0, 5, 0, 0) };
                    HeadersPanel.Children.Add(headerLabel);
                }

                // Try to get and analyze favicon
                await AnalyzeSiteFavicon(UrlTextBox.Text);
            }
            catch (HttpRequestException ex)
            {
                var errorLabel = new Label { Content = $"Error: {ex.Message}", Foreground = System.Windows.Media.Brushes.Red, FontSize = 14 };
                HeadersPanel.Children.Add(errorLabel);
            }
        }

        /// <summary>
        /// Downloads and analyzes the favicon from the specified site URL.
        /// </summary>
        /// <param name="siteUrl">The URL of the site to analyze the favicon for.</param>
        private async Task AnalyzeSiteFavicon(string siteUrl)
        {
            try
            {
                var (success, data, message) = await FaviconService.DownloadFaviconAsync(siteUrl);

                if (success && data != null)
                {
                    // Display favicon image
                    using var stream = new MemoryStream(data);
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = stream;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();

                    FaviconImage.Source = bitmap;

                    // Analyze favicon
                    string hash = FaviconService.ComputeMD5FromBytes(data);
                    var (found, framework) = _faviconService.IdentifyFavicon(hash);

                    if (found)
                    {
                        FaviconResultText.Text = $"Identified: {framework}";
                        FaviconResultText.Foreground = System.Windows.Media.Brushes.Green;
                    }
                    else
                    {
                        FaviconResultText.Text = $"Unknown favicon (Hash: {hash})";
                        FaviconResultText.Foreground = System.Windows.Media.Brushes.Orange;
                    }

                    FaviconSection.Visibility = Visibility.Visible;
                }
                else
                {
                    FaviconResultText.Text = $"Could not retrieve favicon: {message}";
                    FaviconResultText.Foreground = System.Windows.Media.Brushes.Red;
                    FaviconSection.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                FaviconResultText.Text = $"Error analyzing favicon: {ex.Message}";
                FaviconResultText.Foreground = System.Windows.Media.Brushes.Red;
                FaviconSection.Visibility = Visibility.Visible;
            }
        }
    }
}