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

        public HomePage()
        {
            InitializeComponent();
            InitializeAlertBar();
        }

        /// <summary>
        /// Initializes the alert bar by reading the alert text file and starting the animation
        /// </summary>
        private void InitializeAlertBar()
        {
            string path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "resources", "alertBarText.txt");
            if (File.Exists(path) && new FileInfo(path).Length > 0)
            {
                UpdateAlertText();
                AlertCanvas.Visibility = Visibility.Visible;
                StartScrollingAnimation();
            }
            else
            {
                AlertCanvas.Visibility = Visibility.Collapsed;
            }
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
                // Update animation if text changed
                if (_scrollingAnimation != null)
                {
                    AlertText.BeginAnimation(Canvas.LeftProperty, null); // Stop the current animation
                }
                SetupScrollingAnimation();
            }
            else
            {
                AlertText.Text = string.Empty;
                AlertCanvas.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Sets up the scrolling animation for the alert text
        /// </summary>
        private void SetupScrollingAnimation()
        {
            if (string.IsNullOrEmpty(AlertText.Text)) return;

            AlertText.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double textWidth = AlertText.DesiredSize.Width;
            double canvasWidth = AlertCanvas.ActualWidth;

            if (textWidth <= canvasWidth) return; // No need to scroll if text fits

            // Position text to the right
            Canvas.SetLeft(AlertText, canvasWidth);

            // Create animation: from right to left (from canvasWidth to -textWidth)
            DoubleAnimation animation = new DoubleAnimation(
                canvasWidth,
                -textWidth,
                TimeSpan.FromSeconds((canvasWidth + textWidth) / 100.0) // ~100 pixels/sec
            );
            animation.Completed += OnScrollingAnimationCompleted;

            // Store animation reference for later use
            _scrollingAnimation = animation;

            // Start animation
            AlertText.BeginAnimation(Canvas.LeftProperty, animation);
        }

        /// <summary>
        /// Starts the scrolling animation for the alert text.
        /// </summary>
        private void StartScrollingAnimation()
        {
            SetupScrollingAnimation();
        }

        /// <summary>
        /// Handles the completion of the scrolling animation by resetting and restarting it
        /// </summary>
        private void OnScrollingAnimationCompleted(object? sender, EventArgs e)
        {
            SetupScrollingAnimation(); // Reset and start again
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
