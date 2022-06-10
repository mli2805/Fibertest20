using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class MeasurementProgressViewModel : PropertyChangedBase
    {
        private Visibility _controlVisibility = Visibility.Collapsed;
        public Visibility ControlVisibility
        {
            get => _controlVisibility;
            set
            {
                if (value == _controlVisibility) return;
                _controlVisibility = value;
                NotifyOfPropertyChange();
            }
        }

        private string _message1;
        public string Message1
        {
            get => _message1;
            set
            {
                if (value == _message1) return;
                _message1 = value;
                NotifyOfPropertyChange();
            }
        }

        private string _message = "";
        public string Message
        {
            get => _message;
            set
            {
                if (value == _message) return;
                _message = value;
                NotifyOfPropertyChange();
            }
        }

        private bool _isCancelButtonEnabled;
        public bool IsCancelButtonEnabled
        {
            get => _isCancelButtonEnabled;
            set
            {
                if (value == _isCancelButtonEnabled) return;
                _isCancelButtonEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        public void DisplayStartMeasurement(string traceTitle)
        {
            Message1 = traceTitle;
            ControlVisibility = Visibility.Visible;
            IsCancelButtonEnabled = true;
            Message = Resources.SID_Sending_command__Wait_please___;

        }
    }
}
