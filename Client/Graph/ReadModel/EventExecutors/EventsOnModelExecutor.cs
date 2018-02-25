namespace Iit.Fibertest.Graph
{
    public class EventsOnModelExecutor
    {
        private readonly EquipmentEventsOnModelExecutor _equipmentEventsOnModelExecutor;
        private readonly NodeEventsOnModelExecutor _nodeEventsOnModelExecutor;
        private readonly FiberEventsOnModelExecutor _fiberEventsOnModelExecutor;
        private readonly TraceEventsOnModelExecutor _traceEventsOnModelExecutor;
        private readonly RtuEventsOnModelExecutor _rtuEventsOnModelExecutor;
        private readonly UserEventsOnModelExecutor _userEventsOnModelExecutor;
        private readonly ZoneEventsOnModelExecutor _zoneEventsOnModelExecutor;
        private readonly EchoEventsOnModelExecutor _echoEventsOnModelExecutor;

        public EventsOnModelExecutor(EquipmentEventsOnModelExecutor equipmentEventsOnModelExecutor, NodeEventsOnModelExecutor nodeEventsOnModelExecutor,
            FiberEventsOnModelExecutor fiberEventsOnModelExecutor, TraceEventsOnModelExecutor traceEventsOnModelExecutor,
            RtuEventsOnModelExecutor rtuEventsOnModelExecutor, UserEventsOnModelExecutor userEventsOnModelExecutor,
            ZoneEventsOnModelExecutor zoneEventsOnModelExecutor, EchoEventsOnModelExecutor echoEventsOnModelExecutor)
        {
            _equipmentEventsOnModelExecutor = equipmentEventsOnModelExecutor;
            _nodeEventsOnModelExecutor = nodeEventsOnModelExecutor;
            _fiberEventsOnModelExecutor = fiberEventsOnModelExecutor;
            _traceEventsOnModelExecutor = traceEventsOnModelExecutor;
            _rtuEventsOnModelExecutor = rtuEventsOnModelExecutor;
            _userEventsOnModelExecutor = userEventsOnModelExecutor;
            _zoneEventsOnModelExecutor = zoneEventsOnModelExecutor;
            _echoEventsOnModelExecutor = echoEventsOnModelExecutor;
        }

        public void Apply(object e)
        {
            switch (e)
            {
                case NodeIntoFiberAdded evnt: _nodeEventsOnModelExecutor.AddNodeIntoFiber(evnt); return;
                case NodeUpdated evnt: _nodeEventsOnModelExecutor.UpdateNode(evnt); return;
                case NodeMoved evnt: _nodeEventsOnModelExecutor.MoveNode(evnt); return;
                case NodeRemoved evnt: _nodeEventsOnModelExecutor.RemoveNode(evnt); return;

                case FiberAdded evnt: _fiberEventsOnModelExecutor.AddFiber(evnt); return;
                case FiberUpdated evnt: _fiberEventsOnModelExecutor.UpdateFiber(evnt); return;
                case FiberRemoved evnt: _fiberEventsOnModelExecutor.RemoveFiber(evnt); return;

                case EquipmentIntoNodeAdded evnt: _equipmentEventsOnModelExecutor.AddEquipmentIntoNode(evnt); return;
                case EquipmentAtGpsLocationAdded evnt: _equipmentEventsOnModelExecutor.AddEquipmentAtGpsLocation(evnt); return;
                case EquipmentUpdated evnt: _equipmentEventsOnModelExecutor.UpdateEquipment(evnt); return;
                case EquipmentRemoved evnt: _equipmentEventsOnModelExecutor.RemoveEquipment(evnt); return;

                case RtuAtGpsLocationAdded evnt: _rtuEventsOnModelExecutor.AddRtuAtGpsLocation(evnt); return;
                case RtuUpdated evnt: _rtuEventsOnModelExecutor.UpdateRtu(evnt); return;
                case RtuRemoved evnt: _rtuEventsOnModelExecutor.RemoveRtu(evnt); return;
                case OtauAttached evnt: _rtuEventsOnModelExecutor.AttachOtau(evnt); return;
                case OtauDetached evnt: _rtuEventsOnModelExecutor.DetachOtau(evnt); return;

                case TraceAdded evnt: _traceEventsOnModelExecutor.AddTrace(evnt); return;
                case TraceUpdated evnt: _traceEventsOnModelExecutor.UpdateTrace(evnt); return;
                case TraceCleaned evnt: _traceEventsOnModelExecutor.CleanTrace(evnt); return;
                case TraceRemoved evnt: _traceEventsOnModelExecutor.RemoveTrace(evnt); return;
                case TraceAttached evnt: _traceEventsOnModelExecutor.AttachTrace(evnt); return;
                case TraceDetached evnt: _traceEventsOnModelExecutor.DetachTrace(evnt); return;

                case UserAdded evnt: _userEventsOnModelExecutor.AddUser(evnt); return;
                case UserUpdated evnt: _userEventsOnModelExecutor.UpdateUser(evnt); return;
                case UserRemoved evnt: _userEventsOnModelExecutor.RemoveUser(evnt); return;

                case ZoneAdded evnt: _zoneEventsOnModelExecutor.AddZone(evnt); return;
                case ZoneUpdated evnt: _zoneEventsOnModelExecutor.UpdateZone(evnt); return;
                case ZoneRemoved evnt: _zoneEventsOnModelExecutor.RemoveZone(evnt); return;

                case BaseRefAssigned evnt: _echoEventsOnModelExecutor.AssignBaseRef(evnt); return;
                case RtuInitialized evnt: _echoEventsOnModelExecutor.InitializeRtu(evnt); return;
                case MonitoringSettingsChanged evnt: _echoEventsOnModelExecutor.ChangeMonitoringSettings(evnt); return;
                case MonitoringStarted evnt: _echoEventsOnModelExecutor.StartMonitoring(evnt); return;
                case MonitoringStopped evnt: _echoEventsOnModelExecutor.StopMonitoring(evnt); return;

                default: return;
            }
        }
    }
}