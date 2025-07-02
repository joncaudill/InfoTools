using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace InfoTools
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : Page
    {
        private DoubleAnimation? _scrollingAnimation;
        private bool _isScrolling;
        private double _scrollSpeed = 2.0; // pixels per frame, adjust as needed

        public HomePage()
        {
            InitializeComponent();
            this.Loaded += HomePage_Loaded;
        }

        private void HomePage_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeAlertBar();
        }

        /// <summary>
        /// Initializes the alert bar by reading the alert text file and starting the animation
        /// </summary>
        private void InitializeAlertBar()
        {
            string path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "resources", "alertBarText.txt");
            UpdateAlertText(); // This will set visibility appropriately

            // Start timer to check for updates every minute
            var _alertTimer = new System.Timers.Timer(60000);
            _alertTimer.Elapsed += OnAlertTimerElapsed;
            _alertTimer.AutoReset = true;
            _alertTimer.Start();
        }

        /// <summary>
        /// Updates the alert text by reading the file and replacing placeholders
        /// </summary>
        private void UpdateAlertText()
        {
            string path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "resources", "alertBarText.txt");
            if (File.Exists(path) && new FileInfo(path).Length > 0)
            {
                string text = File.ReadAllText(path);
                text = text.Replace("$$DAY$$", DateTime.Now.DayOfWeek.ToString());
                text = text.Replace("$$MONTH$$", DateTime.Now.ToString("MMMM"));
                text = text.Replace("$$DATE$$", DateTime.Now.ToShortDateString());
                text = text.Replace("$$YEAR$$", DateTime.Now.Year.ToString());
                AlertText.Text = text;
                if (text.Length > 0)
                {
                    AlertCanvas.Visibility = Visibility.Visible; // Only show if text is present
                }
                else
                {
                    AlertCanvas.Visibility = Visibility.Hidden;
                }
                StopScrollingAnimation();
                SetupScrollingAnimation();
            }
            else
            {
                AlertText.Text = string.Empty;
                AlertCanvas.Visibility = Visibility.Collapsed;
                StopScrollingAnimation();
            }
        }

        /// <summary>
        /// Sets up the scrolling animation for the alert text
        /// </summary>
        private void SetupScrollingAnimation()
        {
            if (string.IsNullOrEmpty(AlertText.Text)) return;

            AlertCanvas.Visibility = Visibility.Visible;
            AlertCanvas.UpdateLayout();

            AlertText.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double textWidth = AlertText.DesiredSize.Width;
            double canvasWidth = AlertCanvas.ActualWidth;

            if (canvasWidth == 0)
            {
                AlertCanvas.Dispatcher.BeginInvoke(
                    new Action(SetupScrollingAnimation),
                    System.Windows.Threading.DispatcherPriority.Loaded
                );
                return;
            }

            //if (textWidth <= canvasWidth)
            //{
            //    StopScrollingAnimation();
            //    return;
            //}

            Canvas.SetLeft(AlertText, canvasWidth);

            if (!_isScrolling)
            {
                CompositionTarget.Rendering += OnAlertTextRender;
                _isScrolling = true;
            }
        }

        /// <summary>
        /// Handles the rendering event to scroll the alert text
        /// </summary>
        private void OnAlertTextRender(object? sender, EventArgs e)
        {
            double textWidth = AlertText.DesiredSize.Width;
            double canvasWidth = AlertCanvas.ActualWidth;
            double left = Canvas.GetLeft(AlertText);

            // Move left by scroll speed
            left -= _scrollSpeed;
            Canvas.SetLeft(AlertText, left);

            // If the right edge of the text is off the left side, reset to right
            if (left + textWidth <= 0)
            {
                Canvas.SetLeft(AlertText, canvasWidth);
            }
        }

        /// <summary>
        /// Stops the scrolling animation for the alert text
        /// </summary>
        private void StopScrollingAnimation()
        {
            if (_isScrolling)
            {
                CompositionTarget.Rendering -= OnAlertTextRender;
                _isScrolling = false;
            }
        }

        /// <summary>
        /// Starts the scrolling animation for the alert text.
        /// </summary>
        private void StartScrollingAnimation()
        {
            SetupScrollingAnimation();
        }

        /// <summary>
        /// Event handler for the alert timer that checks for file updates every 60 seconds
        /// </summary>
        private void OnAlertTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() => UpdateAlertText());
        }
    }
}
