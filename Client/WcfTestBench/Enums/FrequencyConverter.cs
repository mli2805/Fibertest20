using System;
using System.Globalization;
using System.Windows.Data;

namespace WcfTestBench
{
    public class FrequencyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            var instance = (Frequency)value;
            return ConvertToString(instance, (string)parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Empty;
        }

        private static string ConvertToString(Frequency instance, string param)
        {
            switch (instance)
            {
                case Frequency.DoNot: return param == "0" ? "Do not measure" : "Do not save";
                case Frequency.EveryHour: return "Every hour";
                case Frequency.Every6Hours: return "Every 6 hours";
                case Frequency.Every12Hours: return "Every 12 hours";
                case Frequency.EveryDay: return "Every day";
                case Frequency.Every2Days: return "Every 2 days";
                case Frequency.Every7Days: return "Every 7 days";
                case Frequency.Every30Days: return "Every 30 days";
                default: return "Wrong param";
            }
        }
    }

    
}