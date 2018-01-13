using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph.Algorithms;
using Iit.Fibertest.UtilsLib;
using PrivateReflectionUsingDynamic;

namespace Iit.Fibertest.Graph
{
    public class WriteModel
    {
        private readonly IMyLog _logFile;
        public List<object> EventsWaitingForCommit { get; } = new List<object>();
        private readonly IMapper _mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingEventToDomainModelProfile>()).CreateMapper();

        private readonly List<Node> _nodes = new List<Node>();
        private readonly List<Fiber> _fibers = new List<Fiber>();
        private readonly List<Equipment> _equipments = new List<Equipment>();
        private readonly List<Trace> _traces = new List<Trace>();
        private readonly List<Rtu> _rtus = new List<Rtu>();

        public WriteModel(IMyLog logFile)
        {
            _logFile = logFile;
        }

        public void Init(IEnumerable<object> events)
        {
            foreach (var dbEvent in events)
            {
                this.AsDynamic().Apply(dbEvent);
            }
        }

        public string Add(object evnt)
        {
            var result = (string)this.AsDynamic().Apply(evnt);
            if (result == null)
                EventsWaitingForCommit.Add(evnt);
            return result;
        }

        public void Commit()
        {
            EventsWaitingForCommit.Clear();
        }

        public bool HasAnotherNodeWithTitle(string title, Guid id)
        {
            return _nodes.Any(n => n.Title == title && n.Id != id);
        }

        public bool HasFiberBetween(Guid a, Guid b)
        {
            return _fibers.Any(f =>
                f.Node1 == a && f.Node2 == b ||
                f.Node1 == b && f.Node2 == a);
        }

        public Trace GetTrace(Guid traceId)
        {
            return _traces.FirstOrDefault(t => t.Id == traceId);
        }

        public Rtu GetRtu(Guid id)
        {
            return _rtus.FirstOrDefault(r => r.Id == id);
        }

        #region Node
        public string Apply(NodeAdded e)
        {
            _nodes.Add(_mapper.Map<Node>(e));
            return null;
        }

        public string Apply(NodeIntoFiberAdded e)
        {
            _nodes.Add(new Node() { Id = e.Id, Latitude = e.Position.Lat, Longitude = e.Position.Lng });
            _equipments.Add(new Equipment() { Id = e.EquipmentId, Type = e.IsAdjustmentPoint ? EquipmentType.AdjustmentPoint : EquipmentType.EmptyNode, NodeId = e.Id });
            AddTwoFibersToNewNode(e);
            FixTracesWhichContainedOldFiber(e);
            var fiber = _fibers.FirstOrDefault(f => f.Id == e.FiberId);
            if (fiber != null)
            {
                _fibers.Remove(fiber);
                return null;
            }

            var message = $@"NodeIntoFiberAdded: Fiber {e.FiberId.First6()} not found";
            _logFile.AppendLine(message);
            return message;
        }

        private void FixTracesWhichContainedOldFiber(NodeIntoFiberAdded e)
        {
            foreach (var trace in _traces)
            {
                int idx;
                while ((idx = Topo.GetFiberIndexInTrace(trace, _fibers.Single(f => f.Id == e.FiberId))) != -1)
                {
                    trace.Nodes.Insert(idx + 1, e.Id); // GPS location добавляется во все трассы
                    trace.Equipments.Insert(idx + 1, Guid.Empty); // GPS location добавляется во все трассы
                }
            }
        }
        private void AddTwoFibersToNewNode(NodeIntoFiberAdded e)
        {
            var fiber = _fibers.FirstOrDefault(f => f.Id == e.FiberId);
            if (fiber == null)
            {
                _logFile.AppendLine($@"AddTwoFibersToNewNode: Fiber {e.FiberId.First6()} not found");
                return;
            }
            Guid nodeId1 = fiber.Node1;
            Guid nodeId2 = fiber.Node2;

            _fibers.Add(new Fiber() { Id = e.NewFiberId1, Node1 = nodeId1, Node2 = e.Id });
            _fibers.Add(new Fiber() { Id = e.NewFiberId2, Node1 = e.Id, Node2 = nodeId2 });
        }

        public string Apply(NodeUpdated source)
        {
            var node = _nodes.FirstOrDefault(x => x.Id == source.Id);
            if (node != null)
            {
                _mapper.Map(source, node);
                return null;
            }

            var message = $@"NodeUpdated: Node {source.Id.First6()} not found";
            _logFile.AppendLine(message);
            return message;
        }

        public string Apply(NodeMoved cmd)
        {
            var node = _nodes.FirstOrDefault(x => x.Id == cmd.Id);
            if (node != null)
            {
                node.Latitude = cmd.Latitude;
                node.Longitude = cmd.Longitude;
                return null;
            }

            var message = $@"NodeMoved: Node {cmd.Id.First6()} not found";
            _logFile.AppendLine(message);
            return message;
        }

        public string Apply(NodeRemoved e)
        {
            foreach (var trace in _traces.Where(t => t.Nodes.Contains(e.Id)))
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

            return RemoveNodeWithAllHis(e.Id);
        }

        private void ExcludeNodeFromTrace(Trace trace, Guid fiberId, Guid nodeId)
        {
            var idxInTrace = trace.Nodes.IndexOf(nodeId);
            CreateDetourIfAbsent(trace, fiberId, idxInTrace);

            trace.Equipments.RemoveAt(idxInTrace);
            trace.Nodes.RemoveAt(idxInTrace);
        }

        private string RemoveNodeWithAllHis(Guid nodeId)
        {
            _fibers.RemoveAll(f => f.Node1 == nodeId || f.Node2 == nodeId);
            _equipments.RemoveAll(e => e.NodeId == nodeId);
            var node = _nodes.FirstOrDefault(n => n.Id == nodeId);
            if (node != null)
            {
                _nodes.Remove(node);
                return null;
            }

            var message = $@"RemoveNodeWithAllHis: Node {nodeId.First6()} not found";
            _logFile.AppendLine(message);
            return message;

        }

        private void CreateDetourIfAbsent(Trace trace, Guid fiberId, int idxInTrace)
        {
            var nodeBefore = trace.Nodes[idxInTrace - 1];
            var nodeAfter = trace.Nodes[idxInTrace + 1];

            if (!_fibers.Any(f => f.Node1 == nodeBefore && f.Node2 == nodeAfter
                                  || f.Node2 == nodeBefore && f.Node1 == nodeAfter))
                _fibers.Add(new Fiber() { Id = fiberId, Node1 = nodeBefore, Node2 = nodeAfter });
        }

        public bool IsFiberContainedInAnyTraceWithBase(Guid fiberId)
        {
            var tracesWithBase = _traces.Where(t => t.HasAnyBaseRef);
            var fiber = _fibers.FirstOrDefault(f => f.Id == fiberId);
            if (fiber == null)
            {
                _logFile.AppendLine($@"IsFiberContainedInAnyTraceWithBase: Fiber {fiberId.First6()} not found");
            }
            return tracesWithBase.Any(trace => Topo.GetFiberIndexInTrace(trace, fiber) != -1);
        }

        public bool IsNodeContainedInAnyTraceWithBase(Guid nodeId)
        {
            return _traces.Any(t => t.HasAnyBaseRef && t.Nodes.Contains(nodeId));
        }
        public bool IsNodeLastForAnyTrace(Guid nodeId)
        {
            return _traces.Any(t => t.Nodes.Last() == nodeId);
        }
        #endregion

        #region Fiber
        public string Apply(FiberAdded e)
        {
            _fibers.Add(_mapper.Map<Fiber>(e));
            return null;
        }

        public string Apply(FiberUpdated source)
        {
            var fiber = _fibers.FirstOrDefault(f => f.Id == source.Id);
            if (fiber != null)
            {
                fiber.UserInputedLength = source.UserInputedLength;
                return null;
            }

            var message = $@"FiberUpdated: Fiber {source.Id.First6()} not found";
            _logFile.AppendLine(message);
            return message;
        }

        public string Apply(FiberRemoved e)
        {
            var fiber = _fibers.FirstOrDefault(f => f.Id == e.Id);
            if (fiber != null)
            {
                _fibers.Remove(fiber);
                return null;
            }

            var message = $@"FiberRemoved: Fiber {e.Id.First6()} not found";
            _logFile.AppendLine(message);
            return message;
        }
        #endregion

        #region Equipment
        public string Apply(EquipmentIntoNodeAdded e)
        {
            var node = _nodes.FirstOrDefault(n => n.Id == e.NodeId);
            if (node == null)
            {
                var message = $@"EquipmentIntoNodeAdded: Node {e.NodeId.First6()} not found";
                _logFile.AppendLine(message);
                return message;
            }
            _equipments.Add(new Equipment() { Id = e.Id, Type = e.Type, NodeId = e.NodeId });
            return null;
        }

        public string Apply(EquipmentAtGpsLocationAdded e)
        {
            _nodes.Add(new Node() { Id = e.NodeId, Latitude = e.Latitude, Longitude = e.Longitude });

            _equipments.Add(new Equipment() { Id = e.RequestedEquipmentId, Type = e.Type, NodeId = e.NodeId });
            if (e.EmptyNodeEquipmentId != Guid.Empty)
                _equipments.Add(new Equipment() { Id = e.EmptyNodeEquipmentId, Type = EquipmentType.EmptyNode, NodeId = e.NodeId });

            return null;
        }

        public string Apply(EquipmentUpdated cmd)
        {
            var equipment = _equipments.FirstOrDefault(e => e.Id == cmd.Id);
            if (equipment != null)
            {
                equipment.Title = cmd.Title;
                equipment.Type = cmd.Type;
                equipment.CableReserveLeft = cmd.CableReserveLeft;
                equipment.CableReserveRight = cmd.CableReserveRight;
                equipment.Comment = cmd.Comment;
                return null;
            }

            var message = $@"EquipmentUpdated: Equipment {cmd.Id.First6()} not found";
            _logFile.AppendLine(message);
            return message;
        }

        public string Apply(EquipmentRemoved cmd)
        {
            var equipment = _equipments.FirstOrDefault(e => e.Id == cmd.Id);
            if (equipment == null)
            {
                var message = $@"EquipmentRemoved: Equipment {cmd.Id.First6()} not found";
                _logFile.AppendLine(message);
                return message;
            }

            var emptyEquipment = _equipments.FirstOrDefault(e => e.NodeId == equipment.NodeId && e.Type == EquipmentType.EmptyNode);
            if (emptyEquipment == null)
            {
                var message = $@"EquipmentRemoved: There is no empty equipment in node {equipment.NodeId.First6()}";
                _logFile.AppendLine(message);
                return message;
            }

            var traces = _traces.Where(t => t.Equipments.Contains(equipment.Id));
            foreach (var trace in traces)
            {
                var index = trace.Equipments.FindIndex(e => e == equipment.Id);
                trace.Equipments.Insert(index, emptyEquipment.Id);
            }
            _equipments.Remove(equipment);
            return null;

        }
        #endregion

        #region Rtu
        public string Apply(RtuAtGpsLocationAdded e)
        {
            _nodes.Add(new Node() { Id = e.NodeId, Latitude = e.Latitude, Longitude = e.Longitude });
            _rtus.Add(_mapper.Map<Rtu>(e));
            return null;
        }

        public string Apply(RtuUpdated cmd)
        {
            var rtu = _rtus.FirstOrDefault(r => r.Id == cmd.Id);
            if (rtu != null)
            {
                rtu.Title = cmd.Title;
                rtu.Comment = cmd.Comment;
                return null;
            }

            var message = $@"RtuUpdated: RTU {cmd.Id.First6()} not found";
            _logFile.AppendLine(message);
            return message;
        }

        public string Apply(RtuRemoved cmd)
        {
            var rtu = _rtus.FirstOrDefault(r => r.Id == cmd.Id);
            if (rtu != null)
            {
                var nodeId = rtu.NodeId;
                _traces.RemoveAll(t => t.RtuId == rtu.Id);
                _rtus.Remove(rtu);
                RemoveNodeWithAllHis(nodeId);
                return null;
            }

            var message = $@"RtuRemoved: RTU {cmd.Id.First6()} not found";
            _logFile.AppendLine(message);
            return message;
        }

        public string Apply(MonitoringSettingsChanged cmd)
        {
            var rtu = _rtus.FirstOrDefault(r => r.Id == cmd.RtuId);
            if (rtu != null)
            {
                rtu.MonitoringState = cmd.IsMonitoringOn ? MonitoringState.On : MonitoringState.Off;
                rtu.FastSave = cmd.FastSave;
                rtu.PreciseMeas = cmd.PreciseMeas;
                rtu.PreciseSave = cmd.PreciseSave;
                return null;
            }

            var message = $@"MonitoringSettingsChanged: RTU {cmd.RtuId.First6()} not found";
            _logFile.AppendLine(message);
            return message;

        }
        #endregion

        #region Trace
        public string Apply(TraceAdded e)
        {
            _traces.Add(_mapper.Map<Trace>(e));
            return null;
        }

        public string Apply(TraceCleaned e)
        {
            var trace = _traces.FirstOrDefault(t => t.Id == e.Id);
            if (trace != null)
            {
                _traces.Remove(trace);
                return null;
            }
            var message = $@"TraceCleaned: Trace {e.Id} not found";
            _logFile.AppendLine(message);
            return message;
        }

        public string Apply(TraceRemoved e)
        {
            var trace = _traces.FirstOrDefault(t => t.Id == e.Id);
            if (trace != null)
            {
                _traces.Remove(trace);
                return null;
            }
            var message = $@"TraceRemoved: Trace {e.Id} not found";
            _logFile.AppendLine(message);
            return message;
        }

        public string Apply(TraceAttached cmd)
        {
            var trace = _traces.FirstOrDefault(t => t.Id == cmd.TraceId);
            if (trace != null)
            {
                trace.OtauPort = cmd.OtauPortDto;
                return null;
            }
            var message = $@"TraceAttached: Trace {cmd.TraceId} not found";
            _logFile.AppendLine(message);
            return message;
        }

        public string Apply(TraceDetached cmd)
        {
            var trace = _traces.FirstOrDefault(t => t.Id == cmd.TraceId);
            if (trace != null)
            {
                trace.OtauPort = null;
                return null;
            }
            var message = $@"TraceDetached: Trace {cmd.TraceId} not found";
            _logFile.AppendLine(message);
            return message;
        }

        public string Apply(BaseRefAssigned cmd)
        {
            var trace = _traces.FirstOrDefault(t => t.Id == cmd.TraceId);
            if (trace != null)
            {
                foreach (var baseRefDto in cmd.BaseRefs)
                {
                    if (baseRefDto.BaseRefType == BaseRefType.Precise)
                    {
                        trace.PreciseId = baseRefDto.Id;
                        trace.PreciseDuration = baseRefDto.Duration;
                    }
                    if (baseRefDto.BaseRefType == BaseRefType.Fast)
                    {
                        trace.FastId = baseRefDto.Id;
                        trace.FastDuration = baseRefDto.Duration;
                    }
                    if (baseRefDto.BaseRefType == BaseRefType.Additional)
                    {
                        trace.AdditionalId = baseRefDto.Id;
                        trace.AdditionalDuration = baseRefDto.Duration;
                    }
                }
                if (!trace.HasEnoughBaseRefsToPerformMonitoring)
                    trace.IsIncludedInMonitoringCycle = false;
                return null;
            }
            var message = $@"BaseRefAssigned: Trace {cmd.TraceId} not found";
            _logFile.AppendLine(message);
            return message;
        }
        #endregion

        #region JustEchosOfCmdsSentToRtu
        public string Apply(RtuInitialized e)
        {
            var rtu = _rtus.FirstOrDefault(r => r.Id == e.Id);
            if (rtu != null)
            {
                _mapper.Map(e, rtu);
                return null;
            }
            var message = $@"RtuInitialized: RTU {e.Id.First6()} not found";
            _logFile.AppendLine(message);
            return message;
        }

        public string Apply(MonitoringStarted e)
        {
            var rtu = _rtus.FirstOrDefault(r => r.Id == e.RtuId);
            if (rtu != null)
            {
                rtu.MonitoringState = MonitoringState.On;
                return null;
            }
            var message = $@"MonitoringStarted: RTU {e.RtuId.First6()} not found";
            _logFile.AppendLine(message);
            return message;
        }
        public string Apply(MonitoringStopped e)
        {
            var rtu = _rtus.FirstOrDefault(r => r.Id == e.RtuId);
            if (rtu != null)
            {
                rtu.MonitoringState = MonitoringState.Off;
                return null;
            }
            var message = $@"MonitoringStopped: RTU {e.RtuId.First6()} not found";
            _logFile.AppendLine(message);
            return message;
        }
        #endregion
    }
}