using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class EventsOnTreeExecutor
    {
        private readonly RtuEventsOnTreeExecutor _rtuEventsOnTreeExecutor;
        private readonly TraceEventsOnTreeExecutor _traceEventsOnTreeExecutor;
        private readonly EchoEventsOnTreeExecutor _eventsOnTreeExecutor;

        public EventsOnTreeExecutor(RtuEventsOnTreeExecutor rtuEventsOnTreeExecutor, 
            TraceEventsOnTreeExecutor traceEventsOnTreeExecutor, EchoEventsOnTreeExecutor eventsOnTreeExecutor)
        {
            _rtuEventsOnTreeExecutor = rtuEventsOnTreeExecutor;
            _traceEventsOnTreeExecutor = traceEventsOnTreeExecutor;
            _eventsOnTreeExecutor = eventsOnTreeExecutor;
        }

        public void Apply(object e)
        {
            switch (e)
            {
                case RtuAtGpsLocationAdded evnt: _rtuEventsOnTreeExecutor.AddRtuAtGpsLocation(evnt); return;
                case RtuUpdated evnt: _rtuEventsOnTreeExecutor.UpdateRtu(evnt); return;
                case RtuRemoved evnt: _rtuEventsOnTreeExecutor.RemoveRtu(evnt); return;
                case OtauAttached evnt: _rtuEventsOnTreeExecutor.AttachOtau(evnt); return;
                case OtauDetached evnt: _rtuEventsOnTreeExecutor.DetachOtau(evnt); return;
                case NetworkEventAdded evnt: _rtuEventsOnTreeExecutor.AddNetworkEvent(evnt); return;
                case MeasurementAdded evnt: _rtuEventsOnTreeExecutor.AddMeasurement(evnt); return;

                case TraceAdded evnt: _traceEventsOnTreeExecutor.AddTrace(evnt); return;
                case TraceUpdated evnt: _traceEventsOnTreeExecutor.UpdateTrace(evnt); return;
                case TraceCleaned evnt: _traceEventsOnTreeExecutor.CleanTrace(evnt); return;
                case TraceRemoved evnt: _traceEventsOnTreeExecutor.RemoveTrace(evnt); return;
                case TraceAttached evnt: _traceEventsOnTreeExecutor.AttaceTrace(evnt); return;
                case TraceDetached evnt: _traceEventsOnTreeExecutor.DetachTrace(evnt); return;

                case BaseRefAssigned evnt: _eventsOnTreeExecutor.AssignBaseRef(evnt); return;
                case RtuInitialized evnt: _eventsOnTreeExecutor.InitializeRtu(evnt); return;
                case MonitoringSettingsChanged evnt: _eventsOnTreeExecutor.ChangeMonitoringSettings(evnt); return;
                case MonitoringStarted evnt: _eventsOnTreeExecutor.StartMonitoring(evnt); return;
                case MonitoringStopped evnt: _eventsOnTreeExecutor.StopMonitoring(evnt); return;
            }
        }
    }
}