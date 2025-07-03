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
            LoadSettings();
        }

        private void LoadSettings()
        {
            // Set initial value from InfoToolsSettings if present
            if (App.InfoToolsSettings.TryGetValue("NavigationColor", out var navColor))
            {
                NavigationColorTextBox.Text = navColor;
            }
        }

        private void ApplyChangesButton_Click(object sender, RoutedEventArgs e)
        {
            App.InfoToolsSettings["NavigationColor"] = NavigationColorTextBox.Text;
            App.SaveSettings();

            // Immediately update navigation color in MainWindow
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.ApplyNavigationColor(NavigationColorTextBox.Text);
            }

            MessageBox.Show("Settings applied.", "Settings", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
