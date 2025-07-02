using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Net.Http;
using System;

namespace InfoTools
{
    /// <summary>
    /// Service class that provides favicon analysis functionality including hash computation and database lookup.
    /// </summary>
    public class FaviconService
    {
        private readonly Dictionary<string, string> _faviconDatabase = [];
        private bool _isDatabaseLoaded = false;

        /// <summary>
        /// Gets a value indicating whether the favicon database has been successfully loaded.
        /// </summary>
        public bool IsDatabaseLoaded => _isDatabaseLoaded;

        /// <summary>
        /// Gets the number of favicon hashes loaded in the database.
        /// </summary>
        public int DatabaseCount => _faviconDatabase.Count;

        /// <summary>
        /// Loads the favicon database from the CSV file located in the resources directory.
        /// </summary>
        /// <returns>A tuple containing success status and status message.</returns>
        public (bool Success, string Message) LoadFaviconDatabase()
        {
            try
            {
                string resourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources", "favicons-database.csv");
                if (!File.Exists(resourcePath))
                {
                    return (false, "Favicon database not found.");
                }

                _faviconDatabase.Clear();
                int count = 0;
                using (var reader = new StreamReader(resourcePath))
                {
                    string? line;
                    bool isFirstLine = true;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (isFirstLine)
                        {
                            isFirstLine = false;
                            continue; // skip header
                        }
                        var parts = line.Split(',');
                        if (parts.Length < 2)
                            continue;

                        string hash = parts[1].Trim();
                        string framework = parts[^1].Trim(); // last column
                        if (!string.IsNullOrEmpty(hash) && !string.IsNullOrEmpty(framework))
                        {
                            _faviconDatabase[hash.ToLower()] = framework;
                            count++;
                        }
                    }
                }

                _isDatabaseLoaded = true;
                return (true, $"Loaded {count} hashes.");
            }
            catch (Exception ex)
            {
                _isDatabaseLoaded = false;
                return (false, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Computes the MD5 hash of the specified file.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <returns>The MD5 hash as a lowercase string.</returns>
        public static async Task<string> ComputeMD5FromFileAsync(string filePath)
        {
            using var md5 = MD5.Create();
            using var stream = File.OpenRead(filePath);
            var hashBytes = await Task.Run(() => md5.ComputeHash(stream));
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }

        /// <summary>
        /// Computes the MD5 hash of the provided byte array.
        /// </summary>
        /// <param name="data">The byte array to hash.</param>
        /// <returns>The MD5 hash as a lowercase string.</returns>
        public static string ComputeMD5FromBytes(byte[] data)
        {
            var hashBytes = MD5.HashData(data);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }

        /// <summary>
        /// Identifies a favicon by its MD5 hash using the loaded database.
        /// </summary>
        /// <param name="hash">The MD5 hash to look up.</param>
        /// <returns>A tuple containing the identification result and the framework name if found.</returns>
        public (bool Found, string Framework) IdentifyFavicon(string hash)
        {
            if (!_isDatabaseLoaded)
                return (false, "Database not loaded");

            if (_faviconDatabase.TryGetValue(hash.ToLower(), out string? framework) && !string.IsNullOrEmpty(framework))
            {
                return (true, framework);
            }

            return (false, "Unknown");
        }

        /// <summary>
        /// Downloads a favicon from the specified URL.
        /// </summary>
        /// <param name="url">The URL to download the favicon from.</param>
        /// <returns>A tuple containing success status, the favicon data as byte array, and any error message.</returns>
        public static async Task<(bool Success, byte[]? Data, string Message)> DownloadFaviconAsync(string url)
        {
            try
            {
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(10);

                // Try to get favicon from /favicon.ico first
                var faviconUrl = new Uri(new Uri(url), "/favicon.ico").ToString();

                var response = await client.GetAsync(faviconUrl);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsByteArrayAsync();
                    return (true, data, "Favicon downloaded successfully");
                }

                return (false, null, $"Failed to download favicon: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                return (false, null, $"Error downloading favicon: {ex.Message}");
            }
        }
    }
}
