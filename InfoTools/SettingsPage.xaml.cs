using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();
            LoadAvailableFonts();
            LoadSettings();
        }

        private void LoadAvailableFonts()
        {
            // Get all available system fonts
            var fonts = Fonts.SystemFontFamilies.Select(f => f.Source).OrderBy(f => f).ToList();
            FontFaceComboBox.ItemsSource = fonts;
        }

        private void LoadSettings()
        {
            // Set initial value from InfoToolsSettings if present
            if (App.InfoToolsSettings.TryGetValue("NavigationColor", out var navColor))
            {
                NavigationColorTextBox.Text = navColor;
            }

            if (App.InfoToolsSettings.TryGetValue("AlertBarColor", out var alertColor))
            {
                AlertBarColorTextBox.Text = alertColor;
            }

            if (App.InfoToolsSettings.TryGetValue("AlertBarFontFace", out var fontFace))
            {
                FontFaceComboBox.SelectedItem = fontFace;
            }

            if (App.InfoToolsSettings.TryGetValue("AlertBarScaleX", out var scaleX))
            {
                ScaleXTextBox.Text = scaleX;
            }

            if (App.InfoToolsSettings.TryGetValue("AlertBarScaleY", out var scaleY))
            {
                ScaleYTextBox.Text = scaleY;
            }
        }

        private bool ValidateNumericInput(string input, out double value)
        {
            return double.TryParse(input, out value) && value > 0;
        }

        private void ApplyChangesButton_Click(object sender, RoutedEventArgs e)
        {
            // Validate numeric inputs
            if (!ValidateNumericInput(ScaleXTextBox.Text, out double scaleXValue))
            {
                MessageBox.Show("ScaleX must be a valid positive number.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!ValidateNumericInput(ScaleYTextBox.Text, out double scaleYValue))
            {
                MessageBox.Show("ScaleY must be a valid positive number.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Update settings
            App.InfoToolsSettings["NavigationColor"] = NavigationColorTextBox.Text;
            App.InfoToolsSettings["AlertBarColor"] = AlertBarColorTextBox.Text;
            App.InfoToolsSettings["AlertBarFontFace"] = FontFaceComboBox.SelectedItem?.ToString() ?? "Consolas";
            App.InfoToolsSettings["AlertBarScaleX"] = ScaleXTextBox.Text;
            App.InfoToolsSettings["AlertBarScaleY"] = ScaleYTextBox.Text;
            App.SaveSettings();

            // Immediately update navigation color in MainWindow
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.ApplyNavigationColor(NavigationColorTextBox.Text);

                // Update alert bar settings on home page if it's currently displayed
                if (mainWindow.MainFrame.Content is HomePage homePage)
                {
                    homePage.ApplyAlertBarColor(AlertBarColorTextBox.Text);
                    homePage.ApplyAlertBarFontAndScale();
                }
            }

            MessageBox.Show("Settings applied.", "Settings", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
