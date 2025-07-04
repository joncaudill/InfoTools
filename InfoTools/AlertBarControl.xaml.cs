using System;
using System.IO;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace InfoTools
{
    public partial class AlertBarControl : UserControl
    {
        private DoubleAnimation? _scrollingAnimation;
        private bool _isScrolling;
        private double _scrollSpeed = 2.0;
        private System.Timers.Timer? _alertTimer;
        private System.Timers.Timer? _alertTimeUpdateTimer;
        private bool _alertTextHasTimeTag = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="AlertBarControl"/> class.
        /// </summary>
        public AlertBarControl()
        {
            InitializeComponent();
            this.Loaded += AlertBarControl_Loaded;
        }

        /// <summary>
        /// Handles the Loaded event, applying settings and initializing the alert bar.
        /// </summary>
        private void AlertBarControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyAlertBarColorFromSettings();
            ApplyAlertBarFontAndScale();
            InitializeAlertBar();
        }

        /// <summary>
        /// Applies font face and scale settings from configuration to the alert text.
        /// </summary>
        public void ApplyAlertBarFontAndScale()
        {
            if (App.InfoToolsSettings.TryGetValue("AlertBarFontFace", out var fontFace))
            {
                try
                {
                    AlertText.FontFamily = new FontFamily(fontFace);
                }
                catch
                {
                }
            }

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
        /// Applies the specified color to the alert bar background.
        /// </summary>
        /// <param name="colorHex">Hex color string (e.g., "#FF0000").</param>
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
            }
        }

        /// <summary>
        /// Applies the alert bar color from settings.
        /// </summary>
        private void ApplyAlertBarColorFromSettings()
        {
            if (App.InfoToolsSettings.TryGetValue("AlertBarColor", out var colorHex))
            {
                ApplyAlertBarColor(colorHex);
            }
        }

        /// <summary>
        /// Initializes the alert bar by reading the alert text file and starting the update timer.
        /// </summary>
        private void InitializeAlertBar()
        {
            string path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "resources", "alertBarText.txt");
            UpdateAlertText();

            _alertTimer = new System.Timers.Timer(60000);
            _alertTimer.Elapsed += OnAlertTimerElapsed;
            _alertTimer.AutoReset = true;
            _alertTimer.Start();
        }

        /// <summary>
        /// Updates the alert text by reading the file and replacing placeholders.
        /// Starts or stops the per-second timer if $$TIME$$ is present or removed.
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
                    AlertCanvas.Visibility = Visibility.Visible;
                }
                else
                {
                    AlertCanvas.Visibility = Visibility.Hidden;
                }
                StopScrollingAnimation();
                SetupScrollingAnimation();

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
        /// Sets up the scrolling animation for the alert text.
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
        /// Handles the rendering event to scroll the alert text.
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
        /// Stops the scrolling animation for the alert text.
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
        /// Event handler for the alert timer that checks for file updates every 60 seconds.
        /// </summary>
        private void OnAlertTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() => UpdateAlertText());
        }

        /// <summary>
        /// Manages the per-second timer for updating the alert text if $$TIME$$ is present.
        /// </summary>
        /// <param name="enable">True to enable the timer, false to disable and dispose it.</param>
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
        /// Event handler for the per-second timer that updates the alert text if $$TIME$$ is present.
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