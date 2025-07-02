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
            MainFrame.Navigate(new HomePage());
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
    }
}