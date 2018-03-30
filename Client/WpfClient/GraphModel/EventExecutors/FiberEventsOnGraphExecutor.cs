using System;
using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class FiberEventsOnGraphExecutor
    {
        private readonly GraphReadModel _model;
        private readonly CurrentUser _currentUser;

        public FiberEventsOnGraphExecutor(GraphReadModel model, CurrentUser currentUser)
        {
            _model = model;
            _currentUser = currentUser;
        }

        public void AddFiber(FiberAdded evnt)
        {
            if (_currentUser.ZoneId != Guid.Empty) return;

            _model.Data.Fibers.Add(new FiberVm()
            {
                Id = evnt.FiberId,
                Node1 = _model.Data.Nodes.First(m => m.Id == evnt.NodeId1),
                Node2 = _model.Data.Nodes.First(m => m.Id == evnt.NodeId2),
            });
        }

        public void RemoveFiber(FiberRemoved evnt)
        {
            if (_currentUser.ZoneId != Guid.Empty
                && _model.Data.Fibers.All(f => f.Id != evnt.FiberId)) return;

            var fiberVm = _model.Data.Fibers.First(f => f.Id == evnt.FiberId);
            RemoveFiberUptoRealNodesNotPoints(fiberVm);
        }

        private void RemoveFiberUptoRealNodesNotPoints(FiberVm fiber)
        {
            var leftNode = _model.Data.Nodes.First(n => n.Id == fiber.Node1.Id);
            while (leftNode.Type == EquipmentType.AdjustmentPoint)
            {
                var leftFiber = GetAnotherFiberOfAdjustmentPoint(leftNode, fiber.Id);
                _model.Data.Nodes.Remove(leftNode);
                var nextLeftNodeId = leftFiber.Node1.Id == leftNode.Id ? leftFiber.Node2.Id : leftFiber.Node1.Id;
                _model.Data.Fibers.Remove(leftFiber);
                leftNode = _model.Data.Nodes.First(n => n.Id == nextLeftNodeId);
            }

            var rightNode = _model.Data.Nodes.First(n => n.Id == fiber.Node2.Id);
            while (rightNode.Type == EquipmentType.AdjustmentPoint)
            {
                var rightFiber = GetAnotherFiberOfAdjustmentPoint(rightNode, fiber.Id);
                _model.Data.Nodes.Remove(rightNode);
                var nextRightNodeId = rightFiber.Node1.Id == rightNode.Id ? rightFiber.Node2.Id : rightFiber.Node1.Id;
                _model.Data.Fibers.Remove(rightFiber);
                rightNode = _model.Data.Nodes.First(n => n.Id == nextRightNodeId);
            }

            _model.Data.Fibers.Remove(fiber);
        }

        private IEnumerable<FiberVm> GetNodeFibers(NodeVm node)
        {
            foreach (var fiber in _model.Data.Fibers)
                if (fiber.Node1.Id == node.Id || fiber.Node2.Id == node.Id) yield return fiber;
        }

        private FiberVm GetAnotherFiberOfAdjustmentPoint(NodeVm adjustmentPoint, Guid fiberId)
        {
            return GetNodeFibers(adjustmentPoint).First(f => f.Id != fiberId);
        }
    }
}