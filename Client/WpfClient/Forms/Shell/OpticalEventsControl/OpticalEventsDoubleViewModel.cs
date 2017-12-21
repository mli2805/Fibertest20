using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Client
{
    public class OpticalEventsDoubleViewModel : PropertyChangedBase
    {
        private readonly ReadModel _readModel;
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

        public OpticalEventsDoubleViewModel(ReadModel readModel,
            OpticalEventsViewModel allOpticalEventsViewModel,
            OpticalEventsViewModel actualOpticalEventsViewModel)
        {
            _readModel = readModel;
            ActualOpticalEventsViewModel = actualOpticalEventsViewModel;
            ActualOpticalEventsViewModel.TableTitle = @"Actual optical events";
            AllOpticalEventsViewModel = allOpticalEventsViewModel;
            AllOpticalEventsViewModel.TableTitle = @"All optical events";
        }

        public void Apply(Measurement measurement)
        {
            var trace = _readModel.Traces.FirstOrDefault(t => t.Id == measurement.TraceId);
            if (trace == null || !trace.IsAttached)
                return;

            ActualOpticalEventsViewModel.RemoveOldEventForTraceIfExists(measurement.TraceId);

            if (measurement.TraceState == FiberState.Ok)
                return;

            ActualOpticalEventsViewModel.AddEvent(measurement);
        }

        public void ApplyToTableAll(Measurement measurement)
        {
            AllOpticalEventsViewModel.AddEvent(measurement);
        }
    }
}
