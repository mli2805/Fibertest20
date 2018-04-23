using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Animation;
using AutoMapper;
using GMap.NET;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Graph
{
    public class NodeEventsOnModelExecutor
    {
        private static readonly IMapper Mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingEventToDomainModelProfile>()).CreateMapper();

        private readonly IMyLog _logFile;
        private readonly Model _model;

        public NodeEventsOnModelExecutor(Model model, IMyLog logFile)
        {
            _logFile = logFile;
            _model = model;
        }
        public string AddNodeIntoFiber(NodeIntoFiberAdded e)
        {
            var fiber = _model.Fibers.FirstOrDefault(f => f.FiberId == e.FiberId);
            if (fiber == null)
            {
                var message = $@"NodeIntoFiberAdded: Fiber {e.FiberId.First6()} not found";
                _logFile.AppendLine(message);
                return message;
            }

            _model.Nodes.Add(new Node() { NodeId = e.Id, Position = e.Position, TypeOfLastAddedEquipment = e.InjectionType });
            _model.Equipments.Add(new Equipment() { EquipmentId = e.EquipmentId, Type = e.InjectionType, NodeId = e.Id });
            AddTwoFibersToNewNode(e, fiber);
            FixTracesWhichContainedOldFiber(e);

            _model.Fibers.Remove(fiber);
            return null;
        }
        private void FixTracesWhichContainedOldFiber(NodeIntoFiberAdded e)
        {
            foreach (var trace in _model.Traces)
            {
                int idx;
                while ((idx = _model.GetFiberIndexInTrace(trace, _model.Fibers.First(f => f.FiberId == e.FiberId))) != -1)
                {
                    trace.NodeIds.Insert(idx + 1, e.Id); // GPS location добавляется во все трассы
                    trace.EquipmentIds.Insert(idx + 1, e.EquipmentId);
                }
            }
        }
        private void AddTwoFibersToNewNode(NodeIntoFiberAdded e, Fiber oldFiber)
        {
            Guid nodeId1 = _model.Fibers.First(f => f.FiberId == e.FiberId).NodeId1;
            Guid nodeId2 = _model.Fibers.First(f => f.FiberId == e.FiberId).NodeId2;

            var fiber1 = new Fiber() { FiberId = e.NewFiberId1, NodeId1 = nodeId1, NodeId2 = e.Id };
            foreach (var pair in oldFiber.States)
                fiber1.States.Add(pair.Key, pair.Value);
            foreach (var pair in oldFiber.TracesWithExceededLossCoeff)
                fiber1.TracesWithExceededLossCoeff.Add(pair.Key, pair.Value);
            _model.Fibers.Add(fiber1);

            var fiber2 = new Fiber() { FiberId = e.NewFiberId2, NodeId1 = e.Id, NodeId2 = nodeId2 };
            foreach (var pair in oldFiber.States)
                fiber2.States.Add(pair.Key, pair.Value);
            foreach (var pair in oldFiber.TracesWithExceededLossCoeff)
                fiber2.TracesWithExceededLossCoeff.Add(pair.Key, pair.Value);
            _model.Fibers.Add(fiber2);
        }

        public string UpdateNode(NodeUpdated source)
        {
            Node destination = _model.Nodes.FirstOrDefault(n => n.NodeId == source.NodeId);
            if (destination == null)
            {
                var message = $@"NodeUpdated: Node {source.NodeId.First6()} not found";
                _logFile.AppendLine(message);
                return message;
            }
            Mapper.Map(source, destination);
            return null;
        }

        public string UpdateAndMoveNode(NodeUpdatedAndMoved source)
        {
            Node destination = _model.Nodes.FirstOrDefault(n => n.NodeId == source.NodeId);
            if (destination == null)
            {
                var message = $@"NodeUpdated: Node {source.NodeId.First6()} not found";
                _logFile.AppendLine(message);
                return message;
            }
            Mapper.Map(source, destination);
            return null;
        }

        public string MoveNode(NodeMoved newLocation)
        {
            Node node = _model.Nodes.FirstOrDefault(n => n.NodeId == newLocation.NodeId);
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
            foreach (var trace in _model.Traces.Where(t => t.NodeIds.Contains(e.NodeId)))
            {
                var result = ExcludeAllNodeAppearancesInTrace(e.NodeId, trace, e.DetoursForGraph);
                if (result != null) return result;
            }

            if (e.FiberIdToDetourAdjustmentPoint != Guid.Empty)
                return ExcludeAdjustmentPoint(e.NodeId, e.FiberIdToDetourAdjustmentPoint);

            return e.DetoursForGraph.Count == 0
                ? _model.RemoveNodeWithAllHisFibersUptoRealNode(e.NodeId)
                : _model.RemoveNodeWithAllHisFibers(e.NodeId);
        }

        private string ExcludeAllNodeAppearancesInTrace(Guid nodeId, Trace trace, List<NodeDetour> nodeDetours)
        {
            int index;
            while ((index = trace.NodeIds.IndexOf(nodeId)) != -1)
            {
                var detour = nodeDetours.FirstOrDefault(d =>
                    d.TraceId == trace.TraceId && d.NodeId1 == trace.NodeIds[index - 1] &&
                    d.NodeId2 == trace.NodeIds[index + 1]);
                if (detour == null)
                {
                    var message = $@"NodeRemoved: No fiber prepared to detour trace {trace.TraceId}";
                    _logFile.AppendLine(message);
                    return message;
                }

                if (detour.NodeId1 == detour.NodeId2)
                {
                    var result = ExcludeTurnNodeFromTrace(trace, index);
                    if (result != null) return result;
                }
                else
                {
                    CreateDetourIfAbsent(detour);
                    trace.EquipmentIds.RemoveAt(index);
                    trace.NodeIds.RemoveAt(index);
                }
            }

            return null;
        }

        private string ExcludeTurnNodeFromTrace(Trace trace, int index)
        {
            if (trace.NodeIds.Count <= 3)
            {
                var message = $@"NodeRemoved: Trace {trace.TraceId} is too short to remove turn node";
                _logFile.AppendLine(message);
                return message;
            }
            if (index == 1) index++;

            trace.EquipmentIds.RemoveAt(index);
            trace.NodeIds.RemoveAt(index);
            trace.EquipmentIds.RemoveAt(index - 1);
            trace.NodeIds.RemoveAt(index - 1);

            return null;
        }

        private void CreateDetourIfAbsent(NodeDetour detour)
        {
            var nodeBefore = detour.NodeId1;
            var nodeAfter = detour.NodeId2;

            var fiber = _model.Fibers.FirstOrDefault(f => f.NodeId1 == nodeBefore && f.NodeId2 == nodeAfter
                                                        || f.NodeId2 == nodeBefore && f.NodeId1 == nodeAfter);
            if (fiber == null)
            {
                fiber = new Fiber()
                {
                    FiberId = detour.FiberId,
                    NodeId1 = nodeBefore,
                    NodeId2 = nodeAfter,
                    States = new Dictionary<Guid, FiberState> { { detour.TraceId, detour.TraceState } }
                };
            }
            else
                fiber.States.Add(detour.TraceId, detour.TraceState);
            _model.Fibers.Add(fiber);

        }

        private string ExcludeAdjustmentPoint(Guid nodeId, Guid detourFiberId)
        {
            var leftFiber = _model.Fibers.FirstOrDefault(f => f.NodeId2 == nodeId);
            if (leftFiber == null)
            {
                var message = @"IsFiberContainedInAnyTraceWithBase: Left fiber not found";
                _logFile.AppendLine(message);
                return message;
            }
            var leftNodeId = leftFiber.NodeId1;

            var rightFiber = _model.Fibers.FirstOrDefault(f => f.NodeId1 == nodeId);
            if (rightFiber == null)
            {
                var message = @"IsFiberContainedInAnyTraceWithBase: Right fiber not found";
                _logFile.AppendLine(message);
                return message;
            }
            var rightNodeId = rightFiber.NodeId2;

            _model.Fibers.Remove(leftFiber);
            _model.Fibers.Remove(rightFiber);
            _model.Fibers.Add(new Fiber() { FiberId = detourFiberId, NodeId1 = leftNodeId, NodeId2 = rightNodeId });

            var node = _model.Nodes.FirstOrDefault(n => n.NodeId == nodeId);
            if (node == null)
            {
                var message = $@"RemoveNodeWithAllHis: Node {nodeId.First6()} not found";
                _logFile.AppendLine(message);
                return message;
            }

            _model.Nodes.Remove(node);
            return null;
        }


        //        private void ExcludeNodeFromTrace(Trace trace, Guid fiberId, Guid nodeId)
        //        {
        //            var idxInTrace = trace.NodeIds.IndexOf(nodeId);
        //            CreateDetourIfAbsent(trace, fiberId, idxInTrace);
        //
        //            trace.EquipmentIds.RemoveAt(idxInTrace);
        //            trace.NodeIds.RemoveAt(idxInTrace);
        //        }


    }
}