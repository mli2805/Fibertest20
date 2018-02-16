using System;
using System.Linq;
using GMap.NET;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Client
{
    public class NodeEventsOnGraphExecutor
    {
        private readonly GraphReadModel _model;
        private readonly IMyLog _logFile;

        public NodeEventsOnGraphExecutor(GraphReadModel model, IMyLog logFile)
        {
            _model = model;
            _logFile = logFile;
        }

        public void AddNodeIntoFiber(NodeIntoFiberAdded evnt)
        {
            var nodeVm = new NodeVm()
            {
                Id = evnt.Id,
                Position = new PointLatLng(evnt.Position.Lat, evnt.Position.Lng),

                State = FiberState.Ok,
                Type = evnt.InjectionType,

                GraphVisibilityLevelItem = _model.SelectedGraphVisibilityItem,
            };
            _model.Nodes.Add(nodeVm);
            _model.Equipments.Add(new EquipmentVm() { Id = evnt.EquipmentId, Type = evnt.InjectionType, Node = nodeVm });

            var fiberForDeletion = _model.Fibers.First(f => f.Id == evnt.FiberId);
            AddTwoFibersToNewNode(evnt, fiberForDeletion);
            FixTracesWhichContainedOldFiber(evnt);
            _model.Fibers.Remove(fiberForDeletion);
        }

        private void FixTracesWhichContainedOldFiber(NodeIntoFiberAdded e)
        {
            foreach (var trace in _model.Traces)
            {
                int idx;
                while ((idx = GetFiberIndexInTrace(trace, _model.Fibers.First(f => f.Id == e.FiberId))) != -1)
                {
                    trace.Nodes.Insert(idx + 1, e.Id);
                }
            }
        }
        private int GetFiberIndexInTrace(TraceVm trace, FiberVm fiber)
        {
            var idxInTrace1 = trace.Nodes.IndexOf(fiber.Node1.Id);
            if (idxInTrace1 == -1)
                return -1;
            var idxInTrace2 = trace.Nodes.IndexOf(fiber.Node2.Id);
            if (idxInTrace2 == -1)
                return -1;
            if (idxInTrace2 - idxInTrace1 == 1)
                return idxInTrace1;
            if (idxInTrace1 - idxInTrace2 == 1)
                return idxInTrace2;
            return -1;
        }

        private void AddTwoFibersToNewNode(NodeIntoFiberAdded e, FiberVm oldFiberVm)
        {
            NodeVm node1 = _model.Fibers.First(f => f.Id == e.FiberId).Node1;
            NodeVm node2 = _model.Fibers.First(f => f.Id == e.FiberId).Node2;

            _model.Fibers.Add(new FiberVm() { Id = e.NewFiberId1, Node1 = node1, Node2 = _model.Nodes.First(n => n.Id == e.Id), States = oldFiberVm.States });
            _model.Fibers.Add(new FiberVm() { Id = e.NewFiberId2, Node1 = _model.Nodes.First(n => n.Id == e.Id), Node2 = node2, States = oldFiberVm.States });
        }

        public void MoveNode(NodeMoved evnt)
        {
            var nodeVm = _model.Nodes.FirstOrDefault(n => n.Id == evnt.NodeId);
            if (nodeVm == null)
                return;
            nodeVm.Position = new PointLatLng(evnt.Latitude, evnt.Longitude);
        }

        public void UpdateNode(NodeUpdated evnt)
        {
            var nodeVm = _model.Nodes.FirstOrDefault(n => n.Id == evnt.Id);
            if (nodeVm == null)
                return;
            nodeVm.Title = evnt.Title;
            nodeVm.Comment = evnt.Comment;
        }

        public void RemoveNode(NodeRemoved evnt)
        {
            foreach (var traceVm in _model.Traces.Where(t => t.Nodes.Contains(evnt.Id)))
                ExcludeNodeFromTrace(traceVm, evnt.TraceWithNewFiberForDetourRemovedNode[traceVm.Id], evnt.Id);

            if (evnt.FiberIdToDetourAdjustmentPoint != Guid.Empty)
            {
                ExcludeAdjustmentPoint(evnt.Id, evnt.FiberIdToDetourAdjustmentPoint);
                return;
            }

            if (evnt.TraceWithNewFiberForDetourRemovedNode.Count == 0 &&
                _model.Fibers.Count(f => f.Node1.Id == evnt.Id || f.Node2.Id == evnt.Id) == 1)
                RemoveNodeOnEdgeWhereNoTraces(evnt.Id);
            else
                RemoveNodeWithAllHis(evnt.Id);
        }

        private void ExcludeNodeFromTrace(TraceVm traceVm, Guid fiberId, Guid nodeId)
        {
            var idxInTrace = traceVm.Nodes.IndexOf(nodeId);
            CreateDetourIfAbsent(traceVm, fiberId, idxInTrace);

            traceVm.Nodes.RemoveAt(idxInTrace);
        }

        private void ExcludeAdjustmentPoint(Guid nodeId, Guid detourFiberId)
        {
            var leftFiber = _model.Fibers.FirstOrDefault(f => f.Node2.Id == nodeId);
            if (leftFiber == null)
            {
                _logFile.AppendLine(@"IsFiberContainedInAnyTraceWithBase: Left fiber not found");
                return;
            }
            var leftNode = leftFiber.Node1;

            var rightFiber = _model.Fibers.FirstOrDefault(f => f.Node1.Id == nodeId);
            if (rightFiber == null)
            {
                _logFile.AppendLine(@"IsFiberContainedInAnyTraceWithBase: Right fiber not found");
                return;
            }
            var rightNode = rightFiber.Node2;

            _model.Fibers.Remove(leftFiber);
            _model.Fibers.Remove(rightFiber);
            _model.Fibers.Add(new FiberVm() { Id = detourFiberId, Node1 = leftNode, Node2 = rightNode });

            var node = _model.Nodes.FirstOrDefault(n => n.Id == nodeId);
            if (node == null)
            {
                _logFile.AppendLine($@"RemoveNodeWithAllHis: Node {nodeId.First6()} not found");
                return;
            }

            _model.Nodes.Remove(node);
        }

        private void RemoveNodeOnEdgeWhereNoTraces(Guid nodeId)
        {
            do
            {
                var node = _model.Nodes.First(n => n.Id == nodeId);
                var fiber = _model.Fibers.First(f => f.Node1.Id == nodeId || f.Node2.Id == nodeId);
                var neighbourId = fiber.Node1.Id == nodeId ? fiber.Node2.Id : fiber.Node1.Id;

                _model.Fibers.Remove(fiber);
                var equipmentInNode = _model.Equipments.Where(e => e.Node.Id == nodeId).ToList();
                foreach (var equipmentVm in equipmentInNode)
                    _model.Equipments.Remove(equipmentVm);
                _model.Nodes.Remove(node);

                nodeId = neighbourId;
            }
            while (IsAdjustmentPoint(nodeId));
        }

        private bool IsAdjustmentPoint(Guid nodeId)
        {
            return _model.Equipments.FirstOrDefault(e => e.Node.Id == nodeId && e.Type == EquipmentType.AdjustmentPoint) != null;
        }

        public void RemoveNodeWithAllHis(Guid nodeId)
        {
            foreach (var fiberVm in _model.Fibers.Where(f => f.Node1.Id == nodeId || f.Node2.Id == nodeId).ToList())
                _model.Fibers.Remove(fiberVm);
            foreach (var equipmentVm in _model.Equipments.Where(e => e.Node.Id == nodeId).ToList())
                _model.Equipments.Remove(equipmentVm);

            var nodeVm = _model.Nodes.FirstOrDefault(n => n.Id == nodeId);
            if (nodeVm != null)
                _model.Nodes.Remove(nodeVm);
            else _logFile.AppendLine($@"NodeVm {nodeId.First6()} not found");
        }
        private void CreateDetourIfAbsent(TraceVm traceVm, Guid fiberId, int idxInTrace)
        {
            var nodeBefore = traceVm.Nodes[idxInTrace - 1];
            var nodeAfter = traceVm.Nodes[idxInTrace + 1];

            if (!_model.Fibers.Any(f => f.Node1.Id == nodeBefore && f.Node2.Id == nodeAfter
                                 || f.Node2.Id == nodeBefore && f.Node1.Id == nodeAfter))
            {
                var fiberVm = new FiberVm()
                {
                    Id = fiberId,
                    Node1 = _model.Nodes.First(m => m.Id == nodeBefore),
                    Node2 = _model.Nodes.First(m => m.Id == nodeAfter),
                };
                _model.Fibers.Add(fiberVm);
                fiberVm.SetState(traceVm.Id, traceVm.State);
            }
        }
    }
}