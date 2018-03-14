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
                case MeasurementAdded evnt: _opticalEventsDoubleViewModel.Apply(evnt); return;
                case MeasurementUpdated evnt: _opticalEventsDoubleViewModel.Apply(evnt); return;
                    default: return;
            }
        }

    }
}