using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class OpticalEventsExecutor
    {
        private readonly OpticalEventsDoubleViewModel _opticalEventsDoubleViewModel;

        public OpticalEventsExecutor(OpticalEventsDoubleViewModel opticalEventsDoubleViewModel)
        {
            _opticalEventsDoubleViewModel = opticalEventsDoubleViewModel;
        }

        public void Apply(object e)
        {
            switch (e)
            {
                case MeasurementAdded evnt:
                    _opticalEventsDoubleViewModel.AddMeasurement(evnt); return;
                case MeasurementUpdated evnt:
                    _opticalEventsDoubleViewModel.UpdateMeasurement(evnt); return;
                case TraceRemoved evnt: _opticalEventsDoubleViewModel.RemoveTrace(evnt); return;
                case TraceCleaned evnt: _opticalEventsDoubleViewModel.CleanTrace(evnt); return;

                    default: return;
            }
        }

    }
}