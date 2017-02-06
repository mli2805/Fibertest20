using System.Windows;
using Caliburn.Micro;

namespace Iit.Fibertest.TestBench
{
    public class OneCoorViewModel : PropertyChangedBase
    {
        public string Degrees { get; set; }
        public string Minutes { get; set; }
        public string Seconds { get; set; }

        public Visibility DegreesModeVisibility
        {
            get { return _degreesModeVisibility; }
            set
            {
                if (value == _degreesModeVisibility) return;
                _degreesModeVisibility = value;
                NotifyOfPropertyChange();
            }
        }

        public Visibility DegreesAndMinutesModeVisibility
        {
            get { return _degreesAndMinutesModeVisibility; }
            set
            {
                if (value == _degreesAndMinutesModeVisibility) return;
                _degreesAndMinutesModeVisibility = value;
                NotifyOfPropertyChange();
            }
        }

        public Visibility DegreesMinutesAndSecondsModeVisibility
        {
            get { return _degreesMinutesAndSecondsModeVisibility; }
            set
            {
                if (value == _degreesMinutesAndSecondsModeVisibility) return;
                _degreesMinutesAndSecondsModeVisibility = value;
                NotifyOfPropertyChange();
            }
        }


        private GpsInputMode _gpsInputMode;
        public GpsInputMode GpsInputMode
        {
            get { return _gpsInputMode; }
            set
            {
                _gpsInputMode = value;
                ChangeVisibilities();
            }
        }

        private void ChangeVisibilities()
        {
            switch (_gpsInputMode)
            {
                case GpsInputMode.Degrees:
                    DegreesModeVisibility = Visibility.Visible;
                    DegreesAndMinutesModeVisibility = Visibility.Collapsed;
                    DegreesMinutesAndSecondsModeVisibility = Visibility.Collapsed;
                    break;
                case GpsInputMode.DegreesAndMinutes:
                    DegreesModeVisibility = Visibility.Collapsed;
                    DegreesAndMinutesModeVisibility = Visibility.Visible;
                    DegreesMinutesAndSecondsModeVisibility = Visibility.Collapsed;
                    break;
                case GpsInputMode.DegreesMinutesAndSeconds:
                    DegreesModeVisibility = Visibility.Collapsed;
                    DegreesAndMinutesModeVisibility = Visibility.Collapsed;
                    DegreesMinutesAndSecondsModeVisibility = Visibility.Visible;
                    break;
            }
        }

        private double _value;
        private Visibility _degreesModeVisibility;
        private Visibility _degreesAndMinutesModeVisibility;
        private Visibility _degreesMinutesAndSecondsModeVisibility;

        public double Value
        {
            get { return _value; }
            set
            {
                _value = value;
                ValueToStrings();
            }
        }

        private void ValueToStrings()
        {
            if (GpsInputMode == GpsInputMode.Degrees)
            {
                Degrees = $"{Value:#0.000000}";
            }
            else if (GpsInputMode == GpsInputMode.DegreesAndMinutes)
            {
                int d = (int)Value;
                Degrees = $"{d:#0}";
                double m = (Value - d) * 60;
                Minutes = $"{m:#0.0000}";
            }
            else if (GpsInputMode == GpsInputMode.DegreesMinutesAndSeconds)
            {
                int d = (int)Value;
                Degrees = $"{d:#0}";
                double m = (Value - d) * 60;
                int mi = (int) m;
                Minutes = $"{mi:#0}";
                double s = (m - mi) * 60;
                Seconds = $"{s:#0.00}";
            }
        }
        public OneCoorViewModel(GpsInputMode gpsInputMode, double value)
        {
            Value = value;
            GpsInputMode = gpsInputMode;
        }
    }
}
