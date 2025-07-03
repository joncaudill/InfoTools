using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.IO;
using System;
using System.Collections.Generic;

namespace InfoTools
{
    /// <summary>
    /// Represents cached data for a scanned site.
    /// </summary>
    public class CachedSiteData
    {
        public DateTime CachedAt { get; set; }
        public Dictionary<string, IEnumerable<string>> Headers { get; set; } = new();
        public byte[]? FaviconData { get; set; }
        public string? FaviconMessage { get; set; }
        public bool FaviconSuccess { get; set; }
    }

    /// <summary>
    /// Interaction logic for GetSiteHeadersPage.xaml
    /// </summary>
    public partial class GetSiteHeadersPage : Page
    {
        private readonly FaviconService _faviconService = new();
        private static readonly Dictionary<string, CachedSiteData> _siteCache = new();
        private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(5);

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

                string url = UrlTextBox.Text;
                string cacheKey = GetCacheKey(url);
                
                // Check if we have cached data that's still valid
                if (_siteCache.TryGetValue(cacheKey, out var cachedData) && 
                    DateTime.Now - cachedData.CachedAt < CacheExpiration)
                {
                    // Use cached data
                    DisplayCachedResults(cachedData);
                }
                else
                {
                    // Fetch fresh data
                    await FetchAndCacheResults(url, cacheKey);
                }
            }
            catch (HttpRequestException ex)
            {
                var errorLabel = new Label { Content = $"Error: {ex.Message}", Foreground = System.Windows.Media.Brushes.Red, FontSize = 14 };
                HeadersPanel.Children.Add(errorLabel);
            }
        }

        /// <summary>
        /// Generates a cache key for the given URL by normalizing it.
        /// </summary>
        /// <param name="url">The URL to generate a cache key for.</param>
        /// <returns>A normalized cache key.</returns>
        private static string GetCacheKey(string url)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                // Normalize the URL for caching (remove fragment, normalize case)
                return $"{uri.Scheme.ToLowerInvariant()}://{uri.Host.ToLowerInvariant()}{uri.AbsolutePath}";
            }
            return url.ToLowerInvariant();
        }

        /// <summary>
        /// Displays cached results in the UI.
        /// </summary>
        /// <param name="cachedData">The cached data to display.</param>
        private void DisplayCachedResults(CachedSiteData cachedData)
        {
            // Add cache indicator
            var cacheLabel = new Label 
            { 
                Content = $"[CACHED COPY] - Cached at {cachedData.CachedAt:yyyy-MM-dd HH:mm:ss}", 
                FontSize = 14, 
                FontWeight = FontWeights.Bold,
                Foreground = System.Windows.Media.Brushes.Blue,
                Margin = new Thickness(0, 0, 0, 10)
            };
            HeadersPanel.Children.Add(cacheLabel);

            // Display headers
            foreach (var header in cachedData.Headers)
            {
                var values = string.Join(", ", header.Value);
                var headerLabel = new Label { Content = $"{header.Key}: {values}", FontSize = 14, Margin = new Thickness(0, 5, 0, 0) };
                HeadersPanel.Children.Add(headerLabel);
            }

            // Display favicon if available
            DisplayFaviconResults(cachedData.FaviconData, cachedData.FaviconMessage, cachedData.FaviconSuccess);
        }

        /// <summary>
        /// Fetches fresh data from the site and caches it.
        /// </summary>
        /// <param name="url">The URL to fetch data from.</param>
        /// <param name="cacheKey">The cache key to store the data under.</param>
        private async Task FetchAndCacheResults(string url, string cacheKey)
        {
            using var client = new HttpClient();
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            // Prepare cache data
            var cacheData = new CachedSiteData
            {
                CachedAt = DateTime.Now
            };

            // Store headers
            foreach (var header in response.Headers)
            {
                cacheData.Headers[header.Key] = header.Value;
            }

            // Display headers
            foreach (var header in response.Headers)
            {
                var values = string.Join(", ", header.Value);
                var headerLabel = new Label { Content = $"{header.Key}: {values}", FontSize = 14, Margin = new Thickness(0, 5, 0, 0) };
                HeadersPanel.Children.Add(headerLabel);
            }

            // Try to get and analyze favicon
            var (success, data, message) = await FaviconService.DownloadFaviconAsync(url);
            cacheData.FaviconSuccess = success;
            cacheData.FaviconData = data;
            cacheData.FaviconMessage = message;

            // Display favicon
            DisplayFaviconResults(data, message, success);

            // Cache the results
            _siteCache[cacheKey] = cacheData;
        }

        /// <summary>
        /// Displays favicon results in the UI.
        /// </summary>
        /// <param name="faviconData">The favicon image data.</param>
        /// <param name="message">The favicon message.</param>
        /// <param name="success">Whether the favicon download was successful.</param>
        private void DisplayFaviconResults(byte[]? faviconData, string? message, bool success)
        {
            try
            {
                if (success && faviconData != null)
                {
                    // Display favicon image
                    using var stream = new MemoryStream(faviconData);
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = stream;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();

                    FaviconImage.Source = bitmap;

                    // Analyze favicon
                    string hash = FaviconService.ComputeMD5FromBytes(faviconData);
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