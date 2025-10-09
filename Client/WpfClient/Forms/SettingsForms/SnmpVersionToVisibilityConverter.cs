using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Iit.Fibertest.Client
{
    public class SnmpVersionToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // parameter — ожидаемая версия, при которой элемент виден
            if (value?.ToString() == (parameter?.ToString() ?? ""))
                return Visibility.Visible;

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
