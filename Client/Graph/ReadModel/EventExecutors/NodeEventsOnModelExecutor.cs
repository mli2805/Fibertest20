using System;
using System.Linq;
using AutoMapper;
using GMap.NET;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph.Algorithms;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Graph
{
    public class NodeEventsOnModelExecutor
    {
        private readonly IMapper _mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingEventToDomainModelProfile>()).CreateMapper();

        private readonly IMyLog _logFile;
        private readonly IModel _model;

        public NodeEventsOnModelExecutor(ReadModel model, IMyLog logFile)
        {
            _logFile = logFile;
            _model = model;
        }
        public string AddNodeIntoFiber(NodeIntoFiberAdded e)
        {
            _model.Nodes.Add(new Node() { Id = e.Id, Position = e.Position, TypeOfLastAddedEquipment = e.InjectionType });
            _model.Equipments.Add(new Equipment() { Id = e.EquipmentId, Type = e.InjectionType, NodeId = e.Id });
            AddTwoFibersToNewNode(e);
            FixTracesWhichContainedOldFiber(e);
            var fiber = _model.Fibers.FirstOrDefault(f => f.Id == e.FiberId);
            if (fiber == null)
            {
                var message = $@"NodeIntoFiberAdded: Fiber {e.FiberId.First6()} not found";
                _logFile.AppendLine(message);
                return message;
            }
            _model.Fibers.Remove(fiber);
            return null;
        }
        private void FixTracesWhichContainedOldFiber(NodeIntoFiberAdded e)
        {
            foreach (var trace in _model.Traces)
            {
                int idx;
                while ((idx = Topo.GetFiberIndexInTrace(trace, _model.Fibers.First(f => f.Id == e.FiberId))) != -1)
                {
                    trace.Nodes.Insert(idx + 1, e.Id); // GPS location добавляется во все трассы
                    trace.Equipments.Insert(idx + 1, e.EquipmentId);
                }
            }
        }
        private void AddTwoFibersToNewNode(NodeIntoFiberAdded e)
        {
            Guid nodeId1 = _model.Fibers.First(f => f.Id == e.FiberId).Node1;
            Guid nodeId2 = _model.Fibers.First(f => f.Id == e.FiberId).Node2;

            _model.Fibers.Add(new Fiber() { Id = e.NewFiberId1, Node1 = nodeId1, Node2 = e.Id });
            _model.Fibers.Add(new Fiber() { Id = e.NewFiberId2, Node1 = e.Id, Node2 = nodeId2 });
        }

        public string UpdateNode(NodeUpdated source)
        {
            Node destination = _model.Nodes.FirstOrDefault(n => n.Id == source.Id);
            if (destination == null)
            {
                var message = $@"NodeUpdated: Node {source.Id.First6()} not found";
                _logFile.AppendLine(message);
                return message;
            }
            _mapper.Map(source, destination);
            return null;
        }

        public string MoveNode(NodeMoved newLocation)
        {
            Node node = _model.Nodes.FirstOrDefault(n => n.Id == newLocation.NodeId);
            if (node == null)
            {
                var message = $@"NodeMoved: Node {newLocation.NodeId.First6()} not found";
                _logFile.AppendLine(message);
                return message;
            }
            node.Position = new PointLatLng(newLocation.Latitude, newLocation.Longitude);
            return null;
        }

        public string RemoveNode(NodeRemoved e)
        {
            foreach (var trace in _model.Traces.Where(t => t.Nodes.Contains(e.Id)))
            {
                if (e.TraceWithNewFiberForDetourRemovedNode == null ||
                    !e.TraceWithNewFiberForDetourRemovedNode.ContainsKey(trace.Id))
                {
                    var message = $@"NodeRemoved: No fiber prepared to detour trace {trace.Id}";
                    _logFile.AppendLine(message);
                    return message;
                }
                else
                    ExcludeNodeFromTrace(trace, e.TraceWithNewFiberForDetourRemovedNode[trace.Id], e.Id);
            }

            if (e.FiberIdToDetourAdjustmentPoint != Guid.Empty)
                return ExcludeAdjustmentPoint(e.Id, e.FiberIdToDetourAdjustmentPoint);

            if (e.TraceWithNewFiberForDetourRemovedNode.Count == 0 &&
                _model.Fibers.Count(f => f.Node1 == e.Id || f.Node2 == e.Id) == 1)
                return RemoveNodeOnEdgeWhereNoTraces(e.Id);
            return _model.RemoveNodeWithAllHis(e.Id);
        }

        private void ExcludeNodeFromTrace(Trace trace, Guid fiberId, Guid nodeId)
        {
            var idxInTrace = trace.Nodes.IndexOf(nodeId);
            CreateDetourIfAbsent(trace, fiberId, idxInTrace);

            trace.Equipments.RemoveAt(idxInTrace);
            trace.Nodes.RemoveAt(idxInTrace);
        }

        private string ExcludeAdjustmentPoint(Guid nodeId, Guid detourFiberId)
        {
            var leftFiber = _model.Fibers.FirstOrDefault(f => f.Node2 == nodeId);
            if (leftFiber == null)
            {
                var message = @"IsFiberContainedInAnyTraceWithBase: Left fiber not found";
                _logFile.AppendLine(message);
                return message;
            }
            var leftNodeId = leftFiber.Node1;

            var rightFiber = _model.Fibers.FirstOrDefault(f => f.Node1 == nodeId);
            if (rightFiber == null)
            {
                var message = @"IsFiberContainedInAnyTraceWithBase: Right fiber not found";
                _logFile.AppendLine(message);
                return message;
            }
            var rightNodeId = rightFiber.Node2;

            _model.Fibers.Remove(leftFiber);
            _model.Fibers.Remove(rightFiber);
            _model.Fibers.Add(new Fiber() { Id = detourFiberId, Node1 = leftNodeId, Node2 = rightNodeId });

            var node = _model.Nodes.FirstOrDefault(n => n.Id == nodeId);
            if (node == null)
            {
                var message = $@"RemoveNodeWithAllHis: Node {nodeId.First6()} not found";
                _logFile.AppendLine(message);
                return message;
            }

            _model.Nodes.Remove(node);
            return null;
        }

        private string RemoveNodeOnEdgeWhereNoTraces(Guid nodeId)
        {
            do
            {
                var node = _model.Nodes.First(n => n.Id == nodeId);
                var fiber = _model.Fibers.First(f => f.Node1 == nodeId || f.Node2 == nodeId);
                var neighbourId = fiber.Node1 == nodeId ? fiber.Node2 : fiber.Node1;

                _model.Fibers.Remove(fiber);
                _model.Equipments.RemoveAll(e => e.NodeId == nodeId);
                _model.Nodes.Remove(node);

                nodeId = neighbourId;
            }
            while (IsAdjustmentPoint(nodeId));

            return null;
        }

        private bool IsAdjustmentPoint(Guid nodeId)
        {
            return _model.Equipments.FirstOrDefault(e => e.NodeId == nodeId && e.Type == EquipmentType.AdjustmentPoint) != null;
        }

        private void CreateDetourIfAbsent(Trace trace, Guid fiberId, int idxInTrace)
        {
            var nodeBefore = trace.Nodes[idxInTrace - 1];
            var nodeAfter = trace.Nodes[idxInTrace + 1];

            if (!_model.Fibers.Any(f => f.Node1 == nodeBefore && f.Node2 == nodeAfter
                                        || f.Node2 == nodeBefore && f.Node1 == nodeAfter))
                _model.Fibers.Add(new Fiber() { Id = fiberId, Node1 = nodeBefore, Node2 = nodeAfter });
        }
    }
}