using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace InfoTools
{
    public class HexToBrushConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string hex && !string.IsNullOrWhiteSpace(hex))
            {
                try
                {
                    return (SolidColorBrush)(new BrushConverter().ConvertFrom(hex) ?? Brushes.Transparent);
                }
                catch
                {
                    return Brushes.Transparent;
                }
            }
            return Brushes.Transparent;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}