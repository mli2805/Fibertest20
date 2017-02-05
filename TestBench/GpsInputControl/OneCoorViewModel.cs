using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Iit.Fibertest.TestBench.Properties;

namespace Iit.Fibertest.TestBench
{
    public class OneCoorViewModel : INotifyPropertyChanged
    {
        public string Degrees { get; set; }
        public string Minutes { get; set; }
        public string Seconds { get; set; }

        public string LeftRadioButtonLabel { get; set; }
        public string RightRadioButtonLabel { get; set; }

        public Visibility DegreesModeVisibility { get; set; }
        public Visibility DegreesAndMinutesModeVisibility { get; set; }
        public Visibility DegreesMinutesAndSecondsModeVisibility { get; set; }


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

        private readonly LatOrLng _coorType;
        private double _value;

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
        public OneCoorViewModel(GpsInputMode gpsInputMode, LatOrLng coorType, double value)
        {
            _coorType = coorType;
            Value = value;
            GpsInputMode = gpsInputMode;

            Initialize();
        }

        private void Initialize()
        {
            if (_coorType == LatOrLng.Latitude)
            {
                LeftRadioButtonLabel = "N";
                RightRadioButtonLabel = "S";
            }
            else
            {
                LeftRadioButtonLabel = "E";
                RightRadioButtonLabel = "W";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
