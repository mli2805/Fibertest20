using Iit.Fibertest.Dto;
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
                    if (evnt.EventStatus > EventStatus.JustMeasurementNotAnEvent)
                        _opticalEventsDoubleViewModel.AddMeasurement(evnt);
                    return;
                case MeasurementUpdated evnt:
                    _opticalEventsDoubleViewModel.UpdateMeasurement(evnt); return;

                case TraceAttached evnt:
                    _opticalEventsDoubleViewModel.AttachTrace(evnt); return;
                case TraceDetached evnt:
                    _opticalEventsDoubleViewModel.DetachTrace(evnt); return;

                case TraceRemoved evnt: _opticalEventsDoubleViewModel.RemoveTrace(evnt); return;
                case TraceCleaned evnt: _opticalEventsDoubleViewModel.CleanTrace(evnt); return;

                case RtuUpdated evnt: _opticalEventsDoubleViewModel.UpdateRtu(evnt); return;
                case TraceUpdated evnt: _opticalEventsDoubleViewModel.UpdateTrace(evnt); return;

                case ResponsibilitiesChanged evnt: _opticalEventsDoubleViewModel.ChangeResponsibilities(evnt); return;

                default: return;
            }
        }

    }
}