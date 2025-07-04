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

        // Add a field to track if $$TIME$$ is present and a timer for per-second updates
        private System.Timers.Timer? _alertTimer;
        private System.Timers.Timer? _alertTimeUpdateTimer;
        private bool _alertTextHasTimeTag = false;

        public HomePage()
        {
            InitializeComponent();
            this.Loaded += HomePage_Loaded;
        }

        /// <summary>
        /// Applies the alert bar color from settings
        /// </summary>
        private void HomePage_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeAlertBar();
            ApplyAlertBarColorFromSettings();
            ApplyAlertBarFontAndScale();
        }

        /// <summary>
        /// Applies font face and scale settings from configuration to the alert bar
        /// </summary>
        public void ApplyAlertBarFontAndScale()
        {
            // Apply font face
            if (App.InfoToolsSettings.TryGetValue("AlertBarFontFace", out var fontFace))
            {
                try
                {
                    AlertText.FontFamily = new FontFamily(fontFace);
                }
                catch
                {
                    // If font family is invalid, keep current font
                }
            }

            // Apply scale settings
            if (App.InfoToolsSettings.TryGetValue("AlertBarScaleX", out var scaleXStr) &&
                App.InfoToolsSettings.TryGetValue("AlertBarScaleY", out var scaleYStr))
            {
                if (double.TryParse(scaleXStr, out double scaleX) && double.TryParse(scaleYStr, out double scaleY))
                {
                    if (AlertText.RenderTransform is ScaleTransform existingTransform)
                    {
                        existingTransform.ScaleX = scaleX;
                        existingTransform.ScaleY = scaleY;
                    }
                    else
                    {
                        AlertText.RenderTransform = new ScaleTransform(scaleX, scaleY);
                    }
                }
            }
        }

        /// <summary>
        /// Applies the specified color to the alert bar background
        /// </summary>
        /// <param name="colorHex">Hex color string (e.g., "#FF0000")</param>
        public void ApplyAlertBarColor(string colorHex)
        {
            try
            {
                var converter = new System.Windows.Media.BrushConverter();
                var brush = (Brush)converter.ConvertFromString(colorHex);
                AlertCanvas.Background = brush;
            }
            catch
            {
                // If color conversion fails, keep the current color
            }
        }

        /// <summary>
        /// Applies the alert bar color from settings
        /// </summary>
        private void ApplyAlertBarColorFromSettings()
        {
            if (App.InfoToolsSettings.TryGetValue("AlertBarColor", out var colorHex))
            {
                ApplyAlertBarColor(colorHex);
            }
        }

        /// <summary>
        /// Initializes the alert bar by reading the alert text file and starting the animation
        /// </summary>
        private void InitializeAlertBar()
        {
            string path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "resources", "alertBarText.txt");
            UpdateAlertText(); // This will set visibility appropriately

            // Start timer to check for updates every minute
            _alertTimer = new System.Timers.Timer(60000);
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
                _alertTextHasTimeTag = text.Contains("$$TIME$$", StringComparison.OrdinalIgnoreCase);

                text = text.Replace("$$DAY$$", DateTime.Now.DayOfWeek.ToString());
                text = text.Replace("$$MONTH$$", DateTime.Now.ToString("MMMM"));
                text = text.Replace("$$DATE$$", DateTime.Now.Day.ToString());
                text = text.Replace("$$YEAR$$", DateTime.Now.Year.ToString());
                text = text.Replace("$$TIME$$", DateTime.Now.ToString("hh:mm:ss tt"));
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

                // Start or stop the per-second timer based on $$TIME$$ tag
                SetupTimeUpdateTimer(_alertTextHasTimeTag);
            }
            else
            {
                AlertText.Text = string.Empty;
                AlertCanvas.Visibility = Visibility.Collapsed;
                StopScrollingAnimation();
                SetupTimeUpdateTimer(false);
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
            double scaleX = 1.0;
            if (AlertText.RenderTransform is ScaleTransform scale)
                scaleX = scale.ScaleX;

            double textWidth = AlertText.DesiredSize.Width * scaleX;
            double canvasWidth = AlertCanvas.ActualWidth;

            if (canvasWidth == 0)
            {
                AlertCanvas.Dispatcher.BeginInvoke(
                    new Action(SetupScrollingAnimation),
                    System.Windows.Threading.DispatcherPriority.Loaded
                );
                return;
            }

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
            double scaleX = 1.0;
            if (AlertText.RenderTransform is ScaleTransform scale)
                scaleX = scale.ScaleX;

            double textWidth = AlertText.DesiredSize.Width * scaleX;
            double canvasWidth = AlertCanvas.ActualWidth;
            double left = Canvas.GetLeft(AlertText);

            left -= _scrollSpeed;
            Canvas.SetLeft(AlertText, left);

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

        /// <summary>
        /// Add this helper to manage the per-second timer
        /// </summary>
        private void SetupTimeUpdateTimer(bool enable)
        {
            if (enable)
            {
                if (_alertTimeUpdateTimer == null)
                {
                    _alertTimeUpdateTimer = new System.Timers.Timer(1000);
                    _alertTimeUpdateTimer.Elapsed += OnAlertTimeUpdateTimerElapsed;
                    _alertTimeUpdateTimer.AutoReset = true;
                    _alertTimeUpdateTimer.Start();
                }
                else
                {
                    _alertTimeUpdateTimer.Start();
                }
            }
            else
            {
                if (_alertTimeUpdateTimer != null)
                {
                    _alertTimeUpdateTimer.Stop();
                    _alertTimeUpdateTimer.Dispose();
                    _alertTimeUpdateTimer = null;
                }
            }
        }

        /// <summary>
        /// This event updates only the $$TIME$$ tag in the alert text
        /// </summary>
        private void OnAlertTimeUpdateTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                string path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "resources", "alertBarText.txt");
                if (File.Exists(path) && new FileInfo(path).Length > 0)
                {
                    string template = File.ReadAllText(path);
                    if (template.Contains("$$TIME$$", StringComparison.OrdinalIgnoreCase))
                    {
                        string text = template.Replace("$$DAY$$", DateTime.Now.DayOfWeek.ToString())
                                              .Replace("$$MONTH$$", DateTime.Now.ToString("MMMM"))
                                              .Replace("$$DATE$$", DateTime.Now.Day.ToString())
                                              .Replace("$$YEAR$$", DateTime.Now.Year.ToString())
                                              .Replace("$$TIME$$", DateTime.Now.ToString("hh:mm:ss tt"));
                        AlertText.Text = text;
                    }
                }
            });
        }
    }
}
