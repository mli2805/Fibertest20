using System.Collections.ObjectModel;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.Dto;

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

        public ObservableCollection<OpticalEvent> Rows { get; set; } = new ObservableCollection<OpticalEvent>();

        public string Message { get; set; } = @"Hello, World!";
    }
}
