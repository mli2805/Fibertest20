using System.Windows;
using Caliburn.Micro;

namespace Iit.Fibertest.Client
{
    public class OpticalEventsViewModel : PropertyChangedBase
    {
        private Visibility _opticalEventsVisiblility;
        public Visibility OpticalEventsVisiblility
        {
            get { return _opticalEventsVisiblility; }
            set
            {
                if (value == _opticalEventsVisiblility) return;
                _opticalEventsVisiblility = value;
                NotifyOfPropertyChange();
            }
        }

        public string Message { get; set; } = @"Hello, World!";
    }
}
