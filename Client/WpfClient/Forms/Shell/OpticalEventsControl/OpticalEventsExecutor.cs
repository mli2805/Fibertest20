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
                    break;
                case MeasurementUpdated evnt:
                    _opticalEventsDoubleViewModel.UpdateMeasurement(evnt); break;

                case TraceAttached evnt:
                    _opticalEventsDoubleViewModel.AttachTrace(evnt); break;
                case TraceDetached evnt:
                    _opticalEventsDoubleViewModel.DetachTrace(evnt); break;

                case TraceRemoved evnt: _opticalEventsDoubleViewModel.RemoveTrace(evnt); break;
                case TraceCleaned evnt: _opticalEventsDoubleViewModel.CleanTrace(evnt); break;

                case RtuUpdated evnt: _opticalEventsDoubleViewModel.UpdateRtu(evnt); break;
                case TraceUpdated evnt: _opticalEventsDoubleViewModel.UpdateTrace(evnt); break;

                case ResponsibilitiesChanged evnt: _opticalEventsDoubleViewModel.ChangeResponsibilities(evnt); break;
            }

            SendSuperClientServerState();
        }

        private void SendSuperClientServerState()
        {

        }

    }
}