﻿using System;
using System.Collections.Generic;
using System.Linq;
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
                if (e.DetoursForGraph == null ||
                    e.DetoursForGraph.All(d => d.TraceId != trace.TraceId))
                {
                    var message = $@"NodeRemoved: No fiber prepared to detour trace {trace.TraceId}";
                    _logFile.AppendLine(message);
                    return message;
                }
                else
                    ExcludeNodeFromTrace(trace, e.DetoursForGraph.First(d=>d.TraceId == trace.TraceId).FiberId, e.NodeId);
            }

            if (e.FiberIdToDetourAdjustmentPoint != Guid.Empty)
                return ExcludeAdjustmentPoint(e.NodeId, e.FiberIdToDetourAdjustmentPoint);

            return e.DetoursForGraph.Count == 0 
                ? _model.RemoveNodeWithAllHisFibersUptoRealNode(e.NodeId) 
                : _model.RemoveNodeWithAllHisFibers(e.NodeId);
        }

        private void ExcludeNodeFromTrace(Trace trace, Guid fiberId, Guid nodeId)
        {
            var idxInTrace = trace.NodeIds.IndexOf(nodeId);
            CreateDetourIfAbsent(trace, fiberId, idxInTrace);

            trace.EquipmentIds.RemoveAt(idxInTrace);
            trace.NodeIds.RemoveAt(idxInTrace);
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

//        private string RemoveNodeOnEdgeWhereNoTraces(Guid nodeId)
//        {
//            do
//            {
//                var node = _model.Nodes.First(n => n.NodeId == nodeId);
//                var fiber = _model.Fibers.First(f => f.NodeId1 == nodeId || f.NodeId2 == nodeId);
//                var neighbourId = fiber.NodeId1 == nodeId ? fiber.NodeId2 : fiber.NodeId1;
//
//                _model.Fibers.Remove(fiber);
//                _model.Equipments.RemoveAll(e => e.NodeId == nodeId);
//                _model.Nodes.Remove(node);
//
//                nodeId = neighbourId;
//            }
//            while (_model.IsAdjustmentPoint(nodeId));
//
//            return null;
//        }
//
//     

        private void CreateDetourIfAbsent(Trace trace, Guid fiberId, int idxInTrace)
        {
            var nodeBefore = trace.NodeIds[idxInTrace - 1];
            var nodeAfter = trace.NodeIds[idxInTrace + 1];

            if (!_model.Fibers.Any(f => f.NodeId1 == nodeBefore && f.NodeId2 == nodeAfter
                                        || f.NodeId2 == nodeBefore && f.NodeId1 == nodeAfter))
                _model.Fibers.Add(new Fiber()
                {
                    FiberId = fiberId,
                    NodeId1 = nodeBefore,
                    NodeId2 = nodeAfter,
                    States = new Dictionary<Guid, FiberState>{{trace.TraceId, trace.State}}
                });
        }
    }
}