using System.ComponentModel;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using JetBrains.Annotations;

namespace Iit.Fibertest.TestBench
{
    public class OneCoorViewModel : PropertyChangedBase, IDataErrorInfo
    {
        public string Degrees
        {
            get { return _degrees; }
            set
            {
                if (value == _degrees) return;
                _degrees = value;
                NotifyOfPropertyChange();
            }
        }

        public string Minutes
        {
            get { return _minutes; }
            set
            {
                if (value == _minutes) return;
                _minutes = value;
                NotifyOfPropertyChange();
            }
        }

        public string Seconds
        {
            get { return _seconds; }
            set
            {
                if (value == _seconds) return;
                _seconds = value;
                NotifyOfPropertyChange();
            }
        }

        private string _snapshot;

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


        private GpsInputMode _currentGpsInputMode;
        public GpsInputMode CurrentGpsInputMode
        {
            get { return _currentGpsInputMode; }
            set
            {
                var flag = IsChanged();
                var temp = StringsToValue();
                _currentGpsInputMode = value;
                ChangeVisibilities();
                if (flag)
                    _value = temp;
                ValueToStrings();
            }
        }

        private void ChangeVisibilities()
        {
            switch (_currentGpsInputMode)
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

        private Visibility _degreesModeVisibility;
        private Visibility _degreesAndMinutesModeVisibility;
        private Visibility _degreesMinutesAndSecondsModeVisibility;
        private string _degrees;
        private string _minutes;
        private string _seconds;

        private double _value;


        public void ReassignValue(double newValue)
        {
            _value = newValue;
            ValueToStrings();
        }
        private void ValueToStrings()
        {
            if (CurrentGpsInputMode == GpsInputMode.Degrees)
            {
                Degrees = $@"{_value:#0.000000}";
            }
            else if (CurrentGpsInputMode == GpsInputMode.DegreesAndMinutes)
            {
                int d = (int)_value;
                Degrees = $@"{d:#0}";
                double m = (_value - d) * 60;
                Minutes = $@"{m:#0.0000}";
            }
            else if (CurrentGpsInputMode == GpsInputMode.DegreesMinutesAndSeconds)
            {
                int d = (int)_value;
                Degrees = $@"{d:#0}";
                double m = (_value - d) * 60;
                int mi = (int)m;
                Minutes = $@"{mi:#0}";
                double s = (m - mi) * 60;
                Seconds = $@"{s:#0.00}";
            }
            _snapshot = TakeSnapShot();
        }

        private bool IsChanged()
        {
            return TakeSnapShot() != _snapshot;
        }

        private string TakeSnapShot()
        {
            if (CurrentGpsInputMode == GpsInputMode.Degrees) return Degrees;
            if (CurrentGpsInputMode == GpsInputMode.DegreesAndMinutes) return Degrees + Minutes;
            return Degrees + Minutes + Seconds;
        }

        // this function is called if only something was changed by user
        public double StringsToValue()
        {
            if (string.IsNullOrEmpty(Degrees)) Degrees = @"0";
            if (string.IsNullOrEmpty(Minutes)) Minutes = @"0";
            if (string.IsNullOrEmpty(Seconds)) Seconds = @"0";
            
            if (CurrentGpsInputMode == GpsInputMode.Degrees)
                return double.Parse(Degrees);
            if (CurrentGpsInputMode == GpsInputMode.DegreesAndMinutes)
                return double.Parse(Degrees) + double.Parse(Minutes) / 60;
            return double.Parse(Degrees) + double.Parse(Minutes) / 60 + double.Parse(Seconds) / 3600;
        }

        [UsedImplicitly/*by VS designer*/]
        public OneCoorViewModel() { }

        public OneCoorViewModel(GpsInputMode currentGpsInputMode, double value)
        {
            _currentGpsInputMode = currentGpsInputMode;
            ChangeVisibilities();
            _value = value;
            ValueToStrings();
        }

        public string this[string columnName]
        {
            get
            {
                var errorMessage = string.Empty;
                switch (columnName)
                {
                    case "Degrees":
                        if (string.IsNullOrEmpty(Degrees))
                            errorMessage = Resources.SID_Degrees_is_required;
                        double t;
                        if (!double.TryParse(Degrees, out t))
                            errorMessage = Resources.SID_Invalid_input;
                        Error = Resources.SID_Invalid_input;
                        break;
                }
                return errorMessage;
            }
        }

        public string Error { get; set; }
    }
}
