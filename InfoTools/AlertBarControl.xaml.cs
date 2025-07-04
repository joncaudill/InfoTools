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

        public AlertBarControl()
        {
            InitializeComponent();
            this.Loaded += AlertBarControl_Loaded;
        }

        private void AlertBarControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyAlertBarColorFromSettings();
            ApplyAlertBarFontAndScale();
            InitializeAlertBar();
        }

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

        private void ApplyAlertBarColorFromSettings()
        {
            if (App.InfoToolsSettings.TryGetValue("AlertBarColor", out var colorHex))
            {
                ApplyAlertBarColor(colorHex);
            }
        }

        private void InitializeAlertBar()
        {
            string path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "resources", "alertBarText.txt");
            UpdateAlertText();

            _alertTimer = new System.Timers.Timer(60000);
            _alertTimer.Elapsed += OnAlertTimerElapsed;
            _alertTimer.AutoReset = true;
            _alertTimer.Start();
        }

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

        private void StopScrollingAnimation()
        {
            if (_isScrolling)
            {
                CompositionTarget.Rendering -= OnAlertTextRender;
                _isScrolling = false;
            }
        }

        private void StartScrollingAnimation()
        {
            SetupScrollingAnimation();
        }

        private void OnAlertTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() => UpdateAlertText());
        }

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