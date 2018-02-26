using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class EventsOnGraphExecutor
    {
        private readonly NodeEventsOnGraphExecutor _nodeEventsOnGraphExecutor;
        private readonly EquipmentEventsOnGraphExecutor _equipmentsExtor;
        private readonly FiberEventsOnGraphExecutor _fiberEventsOnGraphExecutor;
        private readonly TraceEventsOnGraphExecutor _traceEventsOnGraphExecutor;
        private readonly RtuEventsOnGraphExecutor _rtuEventsOnGraphExecutor;

        public EventsOnGraphExecutor(NodeEventsOnGraphExecutor nodeEventsOnGraphExecutor, FiberEventsOnGraphExecutor fiberEventsOnGraphExecutor,
            EquipmentEventsOnGraphExecutor equipmentsExtor, TraceEventsOnGraphExecutor traceEventsOnGraphExecutor, RtuEventsOnGraphExecutor rtuEventsOnGraphExecutor)
        {
            _nodeEventsOnGraphExecutor = nodeEventsOnGraphExecutor;
            _equipmentsExtor = equipmentsExtor;
            _fiberEventsOnGraphExecutor = fiberEventsOnGraphExecutor;
            _traceEventsOnGraphExecutor = traceEventsOnGraphExecutor;
            _rtuEventsOnGraphExecutor = rtuEventsOnGraphExecutor;
        }

        public void Apply(object e)
        {
            switch (e)
            {
                case NodeIntoFiberAdded evnt: _nodeEventsOnGraphExecutor.AddNodeIntoFiber(evnt); return;
                case NodeMoved evnt: _nodeEventsOnGraphExecutor.MoveNode(evnt); return;
                case NodeUpdated evnt: _nodeEventsOnGraphExecutor.UpdateNode(evnt); return;
                case NodeRemoved evnt: _nodeEventsOnGraphExecutor.RemoveNode(evnt); return;

                case FiberAdded evnt: _fiberEventsOnGraphExecutor.AddFiber(evnt); return;
                case FiberRemoved evnt: _fiberEventsOnGraphExecutor.RemoveFiber(evnt); return;

                case EquipmentAtGpsLocationAdded evnt: _equipmentsExtor.AddEquipmentAtGpsLocation(evnt); return;
                case EquipmentIntoNodeAdded evnt: _equipmentsExtor.AddEquipmentIntoNode(evnt); return;
                case EquipmentUpdated evnt: _equipmentsExtor.UpdateEquipment(evnt); return;
                case EquipmentRemoved evnt: _equipmentsExtor.RemoveEquipment(evnt); return;

                case TraceAdded evnt: _traceEventsOnGraphExecutor.AddTrace(evnt); return;
                case TraceCleaned evnt: _traceEventsOnGraphExecutor.CleanTrace(evnt); return;
                case TraceRemoved evnt: _traceEventsOnGraphExecutor.RemoveTrace(evnt); return;
                case TraceAttached evnt: _traceEventsOnGraphExecutor.AttachTrace(evnt); return;
                case TraceDetached evnt: _traceEventsOnGraphExecutor.DetachTrace(evnt); return;

                case MonitoringResultShown evnt: _traceEventsOnGraphExecutor.ShowMonitoringResult(evnt); return;

                case RtuAtGpsLocationAdded evnt: _rtuEventsOnGraphExecutor.AddRtuAtGpsLocation(evnt); return;
                case RtuUpdated evnt: _rtuEventsOnGraphExecutor. UpdateRtu(evnt); return;
                case RtuRemoved evnt: _rtuEventsOnGraphExecutor.RemoveRtu(evnt); return;

                default: return;
            }
        }

      
    }
}
