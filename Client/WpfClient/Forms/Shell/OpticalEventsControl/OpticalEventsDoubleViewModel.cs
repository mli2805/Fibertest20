using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Client
{
    public class OpticalEventsDoubleViewModel : PropertyChangedBase
    {
        private Visibility _opticalEventsVisibility;
        public Visibility OpticalEventsVisibility
        {
            get { return _opticalEventsVisibility; }
            set
            {
                if (value == _opticalEventsVisibility) return;
                _opticalEventsVisibility = value;
                NotifyOfPropertyChange();
            }
        }

        public OpticalEventsViewModel AllOpticalEventsViewModel { get; set; }
        public OpticalEventsViewModel ActualOpticalEventsViewModel { get; set; }

        public OpticalEventsDoubleViewModel(OpticalEventsViewModel allOpticalEventsViewModel,
            OpticalEventsViewModel actualOpticalEventsViewModel)
        {
            ActualOpticalEventsViewModel = actualOpticalEventsViewModel;
            ActualOpticalEventsViewModel.TableTitle = "Actual optical events";
            AllOpticalEventsViewModel = allOpticalEventsViewModel;
            AllOpticalEventsViewModel.TableTitle = "All optical events";
        }

        public void Apply(Measurement measurement)
        {
            AllOpticalEventsViewModel.AddEvent(measurement);

            ActualOpticalEventsViewModel.RemoveOldEventForTraceIfExists(measurement.TraceId);
            ActualOpticalEventsViewModel.AddEvent(measurement);
        }
    }
}
