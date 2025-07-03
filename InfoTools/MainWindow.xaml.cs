using System.Text;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (InfoTools.App.InfoToolsSettings.TryGetValue("NavigationColor", out var navColor) &&
                !string.IsNullOrWhiteSpace(navColor))
            {
                ApplyNavigationColor(navColor);
            }
            else
            {
                ApplyNavigationColor("#2D2D30");
            }

            // Ensure HomePage is loaded on startup
            if (MainFrame.Content == null)
            {
                MainFrame.Navigate(new HomePage());
            }
        }

        public void ApplyNavigationColor(string colorHex)
        {
            try
            {
                var brush = (SolidColorBrush)(new BrushConverter().ConvertFrom(colorHex) ?? Brushes.Transparent);
                NavigationPanel.Background = brush;
            }
            catch
            {
                NavigationPanel.Background = new SolidColorBrush(Color.FromRgb(45, 45, 48));
            }
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new HomePage());
        }

        private void FaviconIdentifierButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new FaviconIdentifierPage());
        }

        private void GetSiteHeadersButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new GetSiteHeadersPage());
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new SettingsPage());
        }
    }
}