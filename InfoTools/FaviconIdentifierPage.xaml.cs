using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace InfoTools
{
    /// <summary>
    /// Interaction logic for FaviconIdentifierPage.xaml
    /// </summary>
    public partial class FaviconIdentifierPage : Page
    {
        private Dictionary<string, string> favicondb = new();

        public FaviconIdentifierPage()
        {
            InitializeComponent();
            LoadFaviconDatabase();
        }

        private void LoadFaviconDatabase()
        {
            try
            {
                string resourcePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources", "favicons-database.csv");
                if (!File.Exists(resourcePath))
                {
                    StatusText.Text = "Favicon database not found.";
                    return;
                }

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
                            favicondb[hash.ToLower()] = framework;
                            count++;
                        }
                    }
                }

                StatusText.Text = $"Loaded {count} hashes.";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Error: {ex.Message}";
            }
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            string resourcesDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources");

            var dlg = new OpenFileDialog
            {
                Filter = "Icon files (*.ico)|*.ico|All files (*.*)|*.*",
                Title = "Select favicon.ico",
                InitialDirectory = Directory.Exists(resourcesDir) ? resourcesDir : AppDomain.CurrentDomain.BaseDirectory

            };
            if (dlg.ShowDialog() == true)
            {
                FilePathTextBox.Text = dlg.FileName;
                AnalyzeButton.IsEnabled = true;
                ResultText.Text = "";
            }
        }

        private async void AnalyzeButton_Click(object sender, RoutedEventArgs e)
        {
            string filePath = FilePathTextBox.Text;
            if (!File.Exists(filePath))
            {
                ResultText.Text = "Selected file does not exist.";
                return;
            }

            ResultText.Text = "Analyzing...";
            AnalyzeButton.IsEnabled = false;

            string hash = await Task.Run(() => ComputeMD5(filePath));

            if (favicondb.TryGetValue(hash, out string? framework) && !string.IsNullOrEmpty(framework))
            {
                ResultText.Text = $"Identified: {framework}";
            }
            else
            {
                ResultText.Text = $"Icon not identified. {hash}";
            }

            AnalyzeButton.IsEnabled = true;
        }

        private static string ComputeMD5(string filePath)
        {
            using var md5 = MD5.Create();
            using var stream = File.OpenRead(filePath);
            var hashBytes = md5.ComputeHash(stream);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
    }
}
