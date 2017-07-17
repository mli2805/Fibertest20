using System;
using System.Globalization;
using System.Windows.Data;

namespace WcfTestBench
{
    public class MeasureFrequencyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var instance = (MeasFreqs?) value;
            return instance?.MyToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Empty;
        }
    }

    public class SaveFrequencyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var instance = (SaveFreqs?) value;
            return instance?.MyToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Empty;
        }
    }

    public static class FrequencyTo
    {
        public static string MyToString(this MeasFreqs instance)
        {
            switch (instance)
            {
                case MeasFreqs.DoNotMeasure: return "Do not measure";
                case MeasFreqs.EveryHour: return "Every hour";
                case MeasFreqs.Every6Hours: return "Every 6 hours";
                case MeasFreqs.Every12Hours: return "Every 12 hours";
                case MeasFreqs.EveryDay: return "Every day";
                default: return "Wrong param";
            }
        }

//        public static TimeSpan Convert(MeasFreqs param)
//        {
//            switch (param)
//            {
//                case MeasFreqs.DoNotMeasure: return TimeSpan.Zero;
//                case MeasFreqs.EveryHour: return TimeSpan.FromHours(1);
//                case MeasFreqs.Every6Hours: return TimeSpan.FromHours(6);
//                case MeasFreqs.Every12Hours: return TimeSpan.FromHours(12);
//                case MeasFreqs.EveryDay: return TimeSpan.FromHours(24);
//                default: return TimeSpan.Zero;
//            }
//        }


        public static string MyToString(this SaveFreqs instance)
        {
            switch (instance)
            {
                case SaveFreqs.DoNotSave: return "Do not save";
                case SaveFreqs.EveryHour: return "Every hour";
                case SaveFreqs.Every6Hours: return "Every 6 hours";
                case SaveFreqs.Every12Hours: return "Every 12 hours";
                case SaveFreqs.EveryDay: return "Every day";
                case SaveFreqs.Every2Days: return "Every 2 days";
                case SaveFreqs.Every7Days: return "Every 7 days";
                case SaveFreqs.Every30Days: return "Every 30 days";
                default: return "Wrong param";
            }
        }

//        public static TimeSpan Convert(SaveFreqs param)
//        {
//            switch (param)
//            {
//                case SaveFreqs.DoNotSave: return TimeSpan.Zero;
//                case SaveFreqs.EveryHour: return TimeSpan.FromHours(1);
//                case SaveFreqs.Every6Hours: return TimeSpan.FromHours(6);
//                case SaveFreqs.Every12Hours: return TimeSpan.FromHours(12);
//                case SaveFreqs.EveryDay: return TimeSpan.FromDays(1);
//                case SaveFreqs.Every2Days: return TimeSpan.FromDays(2);
//                case SaveFreqs.Every7Days: return TimeSpan.FromDays(7);
//                case SaveFreqs.Every30Days: return TimeSpan.FromDays(30);
//                default: return TimeSpan.Zero;
//            }
//        }

    }
}