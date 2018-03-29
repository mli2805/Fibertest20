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
        private readonly ReadModel _readModel;
        private readonly IMyLog _logFile;

        public NodeEventsOnGraphExecutor(IMyLog logFile, GraphReadModel model, ReadModel readModel)
        {
            _model = model;
            _readModel = readModel;
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
            _model.Data.Nodes.Add(nodeVm);

            var fiberForDeletion = _model.Data.Fibers.First(f => f.Id == evnt.FiberId);
            AddTwoFibersToNewNode(evnt, fiberForDeletion);
            _model.Data.Fibers.Remove(fiberForDeletion);
        }

     
        private void AddTwoFibersToNewNode(NodeIntoFiberAdded e, FiberVm oldFiberVm)
        {
            NodeVm node1 = _model.Data.Fibers.First(f => f.Id == e.FiberId).Node1;
            NodeVm node2 = _model.Data.Fibers.First(f => f.Id == e.FiberId).Node2;

            _model.Data.Fibers.Add(new FiberVm() { Id = e.NewFiberId1, Node1 = node1, Node2 = _model.Data.Nodes.First(n => n.Id == e.Id), States = oldFiberVm.States });
            _model.Data.Fibers.Add(new FiberVm() { Id = e.NewFiberId2, Node1 = _model.Data.Nodes.First(n => n.Id == e.Id), Node2 = node2, States = oldFiberVm.States });
        }

        public void MoveNode(NodeMoved evnt)
        {
            var nodeVm = _model.Data.Nodes.FirstOrDefault(n => n.Id == evnt.NodeId);
            if (nodeVm == null)
                return;
            nodeVm.Position = new PointLatLng(evnt.Latitude, evnt.Longitude);
        }

        public void UpdateNode(NodeUpdated evnt)
        {
            var nodeVm = _model.Data.Nodes.FirstOrDefault(n => n.Id == evnt.Id);
            if (nodeVm == null)
                return;
            nodeVm.Title = evnt.Title;
        }

        public void RemoveNode(NodeRemoved evnt)
        {
            foreach (var detour in evnt.DetoursForGraph)
                CreateDetourIfAbsent(detour);

            if (evnt.FiberIdToDetourAdjustmentPoint != Guid.Empty)
            {
                ExcludeAdjustmentPoint(evnt.Id, evnt.FiberIdToDetourAdjustmentPoint);
                return;
            }

            if (evnt.TraceWithNewFiberForDetourRemovedNode.Count == 0 &&
                _model.Data.Fibers.Count(f => f.Node1.Id == evnt.Id || f.Node2.Id == evnt.Id) == 1)
                RemoveNodeOnEdgeWhereNoTraces(evnt.Id);
            else
                RemoveNodeWithAllHis(evnt.Id);
        }

        private void ExcludeAdjustmentPoint(Guid nodeId, Guid detourFiberId)
        {
            var leftFiber = _model.Data.Fibers.FirstOrDefault(f => f.Node2.Id == nodeId);
            if (leftFiber == null)
            {
                _logFile.AppendLine(@"IsFiberContainedInAnyTraceWithBase: Left fiber not found");
                return;
            }
            var leftNode = leftFiber.Node1;

            var rightFiber = _model.Data.Fibers.FirstOrDefault(f => f.Node1.Id == nodeId);
            if (rightFiber == null)
            {
                _logFile.AppendLine(@"IsFiberContainedInAnyTraceWithBase: Right fiber not found");
                return;
            }
            var rightNode = rightFiber.Node2;

            _model.Data.Fibers.Remove(leftFiber);
            _model.Data.Fibers.Remove(rightFiber);
            _model.Data.Fibers.Add(new FiberVm() { Id = detourFiberId, Node1 = leftNode, Node2 = rightNode });

            var node = _model.Data.Nodes.FirstOrDefault(n => n.Id == nodeId);
            if (node == null)
            {
                _logFile.AppendLine($@"RemoveNodeWithAllHis: Node {nodeId.First6()} not found");
                return;
            }

            _model.Data.Nodes.Remove(node);
        }

        private void RemoveNodeOnEdgeWhereNoTraces(Guid nodeId)
        {
            do
            {
                var node = _model.Data.Nodes.First(n => n.Id == nodeId);
                var fiber = _model.Data.Fibers.First(f => f.Node1.Id == nodeId || f.Node2.Id == nodeId);
                var neighbourId = fiber.Node1.Id == nodeId ? fiber.Node2.Id : fiber.Node1.Id;

                _model.Data.Fibers.Remove(fiber);
                _model.Data.Nodes.Remove(node);

                nodeId = neighbourId;
            }
            while (IsAdjustmentPoint(nodeId));
        }

        private bool IsAdjustmentPoint(Guid nodeId)
        {
            return _model.Data.Nodes.FirstOrDefault(e => e.Id == nodeId && e.Type == EquipmentType.AdjustmentPoint) != null;
        }

        public void RemoveNodeWithAllHis(Guid nodeId)
        {
            foreach (var fiberVm in _model.Data.Fibers.Where(f => f.Node1.Id == nodeId || f.Node2.Id == nodeId).ToList())
                _model.Data.Fibers.Remove(fiberVm);

            var nodeVm = _model.Data.Nodes.FirstOrDefault(n => n.Id == nodeId);
            if (nodeVm != null)
                _model.Data.Nodes.Remove(nodeVm);
            else _logFile.AppendLine($@"NodeVm {nodeId.First6()} not found");
        }

        private void CreateDetourIfAbsent(NodeDetour detour)
        {
            if (!_model.Data.Fibers.Any(f => f.Node1.Id == detour.NodeId1 && f.Node2.Id == detour.NodeId2
                                 || f.Node2.Id == detour.NodeId1 && f.Node1.Id == detour.NodeId2))
            {
                var fiberVm = new FiberVm()
                {
                    Id = detour.FiberId,
                    Node1 = _model.Data.Nodes.First(m => m.Id == detour.NodeId1),
                    Node2 = _model.Data.Nodes.First(m => m.Id == detour.NodeId2),
                };
                _model.Data.Fibers.Add(fiberVm);
                fiberVm.SetState(detour.TraceId, detour.TraceState);
            }
        }
    }
}