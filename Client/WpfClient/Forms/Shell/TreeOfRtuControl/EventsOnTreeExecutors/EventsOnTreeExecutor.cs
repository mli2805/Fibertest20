using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class EventsOnTreeExecutor
    {
        private readonly RtuEventsOnTreeExecutor _rtuEventsOnTreeExecutor;
        private readonly InitializeRtuEventOnTreeExecutor _initializeRtuEventOnTreeExecutor;
        private readonly TraceEventsOnTreeExecutor _traceEventsOnTreeExecutor;
        private readonly EchoEventsOnTreeExecutor _echoEventsOnTreeExecutor;
        private readonly ZoneEventsOnTreeExecutor _zoneEventsOnTreeExecutor;

        public EventsOnTreeExecutor(RtuEventsOnTreeExecutor rtuEventsOnTreeExecutor, InitializeRtuEventOnTreeExecutor initializeRtuEventOnTreeExecutor,
            TraceEventsOnTreeExecutor traceEventsOnTreeExecutor, EchoEventsOnTreeExecutor echoEventsOnTreeExecutor,
            ZoneEventsOnTreeExecutor zoneEventsOnTreeExecutor)
        {
            _rtuEventsOnTreeExecutor = rtuEventsOnTreeExecutor;
            _initializeRtuEventOnTreeExecutor = initializeRtuEventOnTreeExecutor;
            _traceEventsOnTreeExecutor = traceEventsOnTreeExecutor;
            _echoEventsOnTreeExecutor = echoEventsOnTreeExecutor;
            _zoneEventsOnTreeExecutor = zoneEventsOnTreeExecutor;
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

                case RtuInitialized evnt: _initializeRtuEventOnTreeExecutor.InitializeRtu(evnt); return;

                case TraceAdded evnt: _traceEventsOnTreeExecutor.AddTrace(evnt); return;
                case TraceUpdated evnt: _traceEventsOnTreeExecutor.UpdateTrace(evnt); return;
                case TraceCleaned evnt: _traceEventsOnTreeExecutor.CleanTrace(evnt); return;
                case TraceRemoved evnt: _traceEventsOnTreeExecutor.RemoveTrace(evnt); return;
                case TraceAttached evnt: _traceEventsOnTreeExecutor.AttaceTrace(evnt); return;
                case TraceDetached evnt: _traceEventsOnTreeExecutor.DetachTrace(evnt); return;
                case MeasurementAdded evnt: _traceEventsOnTreeExecutor.AddMeasurement(evnt); return;

                case BaseRefAssigned evnt: _echoEventsOnTreeExecutor.AssignBaseRef(evnt); return;
                case MonitoringSettingsChanged evnt: _echoEventsOnTreeExecutor.ChangeMonitoringSettings(evnt); return;
                case MonitoringStarted evnt: _echoEventsOnTreeExecutor.StartMonitoring(evnt); return;
                case MonitoringStopped evnt: _echoEventsOnTreeExecutor.StopMonitoring(evnt); return;

                case ResponsibilitiesChanged evnt: _zoneEventsOnTreeExecutor.ChangeResponsibility(evnt); return;
            }
        }
    }
}