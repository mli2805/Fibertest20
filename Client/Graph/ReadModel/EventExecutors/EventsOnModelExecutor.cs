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

        public string Apply(object e)
        {
            switch (e)
            {
                case NodeIntoFiberAdded evnt: return _nodeEventsOnModelExecutor.AddNodeIntoFiber(evnt);
                case NodeUpdated evnt: return _nodeEventsOnModelExecutor.UpdateNode(evnt); 
                case NodeMoved evnt: return _nodeEventsOnModelExecutor.MoveNode(evnt); 
                case NodeRemoved evnt: return _nodeEventsOnModelExecutor.RemoveNode(evnt); 

                case FiberAdded evnt: return _fiberEventsOnModelExecutor.AddFiber(evnt); 
                case FiberUpdated evnt: return _fiberEventsOnModelExecutor.UpdateFiber(evnt); 
                case FiberRemoved evnt: return _fiberEventsOnModelExecutor.RemoveFiber(evnt); 

                case EquipmentIntoNodeAdded evnt: return _equipmentEventsOnModelExecutor.AddEquipmentIntoNode(evnt); 
                case EquipmentAtGpsLocationAdded evnt: return _equipmentEventsOnModelExecutor.AddEquipmentAtGpsLocation(evnt); 
                case EquipmentUpdated evnt: return _equipmentEventsOnModelExecutor.UpdateEquipment(evnt); 
                case EquipmentRemoved evnt: return _equipmentEventsOnModelExecutor.RemoveEquipment(evnt); 

                case RtuAtGpsLocationAdded evnt: return _rtuEventsOnModelExecutor.AddRtuAtGpsLocation(evnt); 
                case RtuUpdated evnt: return _rtuEventsOnModelExecutor.UpdateRtu(evnt); 
                case RtuRemoved evnt: return _rtuEventsOnModelExecutor.RemoveRtu(evnt); 
                case OtauAttached evnt: return _rtuEventsOnModelExecutor.AttachOtau(evnt); 
                case OtauDetached evnt: return _rtuEventsOnModelExecutor.DetachOtau(evnt); 

                case TraceAdded evnt: return _traceEventsOnModelExecutor.AddTrace(evnt); 
                case TraceUpdated evnt: return _traceEventsOnModelExecutor.UpdateTrace(evnt); 
                case TraceCleaned evnt: return _traceEventsOnModelExecutor.CleanTrace(evnt); 
                case TraceRemoved evnt: return _traceEventsOnModelExecutor.RemoveTrace(evnt); 
                case TraceAttached evnt: return _traceEventsOnModelExecutor.AttachTrace(evnt); 
                case TraceDetached evnt: return _traceEventsOnModelExecutor.DetachTrace(evnt); 

                case UserAdded evnt: return _userEventsOnModelExecutor.AddUser(evnt); 
                case UserUpdated evnt: return _userEventsOnModelExecutor.UpdateUser(evnt); 
                case UserRemoved evnt: return _userEventsOnModelExecutor.RemoveUser(evnt); 

                case ZoneAdded evnt: return _zoneEventsOnModelExecutor.AddZone(evnt); 
                case ZoneUpdated evnt: return _zoneEventsOnModelExecutor.UpdateZone(evnt); 
                case ZoneRemoved evnt: return _zoneEventsOnModelExecutor.RemoveZone(evnt); 

                case BaseRefAssigned evnt: return _echoEventsOnModelExecutor.AssignBaseRef(evnt); 
                case RtuInitialized evnt: return _echoEventsOnModelExecutor.InitializeRtu(evnt); 
                case MonitoringSettingsChanged evnt: return _echoEventsOnModelExecutor.ChangeMonitoringSettings(evnt); 
                case MonitoringStarted evnt: return _echoEventsOnModelExecutor.StartMonitoring(evnt); 
                case MonitoringStopped evnt: return _echoEventsOnModelExecutor.StopMonitoring(evnt); 
                case MonitoringResultShown evnt: return _echoEventsOnModelExecutor.ShowMonitoringResult(evnt); 

                default: return @"Unknown event";
            }
        }
    }
}