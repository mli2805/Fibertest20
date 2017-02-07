using System;
using System.Linq;
using GMap.NET;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Commands;

namespace Iit.Fibertest.TestBench
{
    public partial class ShellViewModel
    {
        private void ApplyToMap(AddNode cmd)
        {
            var nodeVm = new NodeVm()
            {
                Id = cmd.Id,
                State = FiberState.Ok,
                Type = cmd.IsJustForCurvature ? EquipmentType.Invisible : EquipmentType.Well,
                Position = new PointLatLng(cmd.Latitude, cmd.Longitude)
            };
            GraphVm.Nodes.Add(nodeVm);
        }

        private void ApplyToMap(MoveNode cmd)
        {
            var nodeVm = GraphVm.Nodes.Single(n => n.Id == cmd.Id);
            nodeVm.Position = new PointLatLng(cmd.Latitude, cmd.Longitude);
        }


        private RemoveNode PrepareCommand(AskRemoveNode request)
        {
            if (GraphVm.Traces.Any(t => t.Nodes.Last() == request.Id))
                return null; // It's prohibited to remove last node from trace
            if (GraphVm.Traces.Any(t => t.Nodes.Contains(request.Id) && t.HasBase))
                return null; // It's prohibited to remove any node from trace with base ref

            var dictionary = GraphVm.Traces.Where(t => t.Nodes.Contains(request.Id)).ToDictionary(trace => trace.Id, trace => Guid.NewGuid());
            return new RemoveNode { Id = request.Id, TraceFiberPairForDetour = dictionary };

        }
        private void ApplyToMap(RemoveNode cmd)
        {
            var fiberVms = GraphVm.Fibers.Where(e => e.NodeA.Id == cmd.Id || e.NodeB.Id == cmd.Id).ToList();
            foreach (var fiberVm in fiberVms)
            {
                GraphVm.Fibers.Remove(fiberVm);
            }
            GraphVm.Nodes.Remove(GraphVm.Nodes.Single(n => n.Id == cmd.Id));
        }

        private AddNodeIntoFiber PrepareCommand(AskAddNodeIntoFiber request)
        {
            if (IsFiberContainedInAnyTraceWithBase(request.FiberId))
            {
                _windowManager.ShowDialog(new NotificationViewModel("", "It's impossible to change trace with base reflectogram"));
                return null;
            }


            var fiberVms = GraphVm.Fibers.First(f => f.Id == request.FiberId);

            return new AddNodeIntoFiber() {FiberId = request.FiberId};
        }

        private bool IsFiberContainedInAnyTraceWithBase(Guid fiberId)
        {
            var tracesWithBase = GraphVm.Traces.Where(t => t.HasBase);
            var fiber = GraphVm.Fibers.Single(f => f.Id == fiberId);
            foreach (var trace in tracesWithBase)
            {
                if (GetFiberIndexInTrace(trace, fiber) != -1)
                    return true;
            }
            return false;
        }
        private int GetFiberIndexInTrace(TraceVm trace, FiberVm fiber)
        {
            var idxInTrace1 = trace.Nodes.IndexOf(fiber.NodeA.Id);
            if (idxInTrace1 == -1)
                return -1;
            var idxInTrace2 = trace.Nodes.IndexOf(fiber.NodeB.Id);
            if (idxInTrace2 == -1)
                return -1;
            if (idxInTrace2 - idxInTrace1 == 1)
                return idxInTrace1;
            if (idxInTrace1 - idxInTrace2 == 1)
                return idxInTrace2;
            return -1;
        }

        private void ApplyToMap(AddNodeIntoFiber cmd)
        {
            
        }
    }

}
