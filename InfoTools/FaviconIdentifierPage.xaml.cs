using Microsoft.Win32;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace InfoTools
{
    /// <summary>
    /// Interaction logic for FaviconIdentifierPage.xaml
    /// </summary>
    public partial class FaviconIdentifierPage : Page
    {
        private readonly FaviconService _faviconService = new();

        /// <summary>
        /// Initializes a new instance of the FaviconIdentifierPage class.
        /// </summary>
        public FaviconIdentifierPage()
        {
            InitializeComponent();
            LoadFaviconDatabase();
        }

        /// <summary>
        /// Loads the favicon database using the FaviconService.
        /// </summary>
        private void LoadFaviconDatabase()
        {
            var (success, message) = _faviconService.LoadFaviconDatabase();
            StatusText.Text = message;
        }

        /// <summary>
        /// Opens a file dialog to browse and select a favicon.ico file.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The routed event arguments.</param>
        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            string resourcesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources");

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

        /// <summary>
        /// Analyzes the selected favicon.ico file by computing its MD5 hash and looking it up in the database.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The routed event arguments.</param>
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

            try
            {
                string hash = await FaviconService.ComputeMD5FromFileAsync(filePath);
                var (found, framework) = _faviconService.IdentifyFavicon(hash);

                if (found)
                {
                    ResultText.Text = $"Identified: {framework}";
                }
                else
                {
                    ResultText.Text = $"Icon not identified. Hash: {hash}";
                }
            }
            catch (Exception ex)
            {
                ResultText.Text = $"Error analyzing file: {ex.Message}";
            }
            finally
            {
                AnalyzeButton.IsEnabled = true;
            }
        }

        /// <summary>
        /// Handles the DragEnter event for the Grid, allowing file drops if the data is a file.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The drag event arguments.</param>
        private void Grid_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Copy;
            else
                e.Effects = DragDropEffects.None;
        }

        /// <summary>
        /// Handles the DragOver event for the Grid, reusing the DragEnter logic.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The drag event arguments.</param>
        private void Grid_DragOver(object sender, DragEventArgs e)
        {
            Grid_DragEnter(sender, e);
        }

        /// <summary>
        /// Handles the Drop event for the Grid, processing dropped favicon.ico files and populating the file path text box.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The drag event arguments.</param>
        private void Grid_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    FilePathTextBox.Text = files[0];
                    AnalyzeButton.IsEnabled = true;
                    ResultText.Text = "";
                }
            }
        }
    }
}