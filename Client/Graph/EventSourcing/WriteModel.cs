using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
            //            Thread.Sleep(TimeSpan.FromSeconds(10));
            foreach (var dbEvent in events)
            {
                this.AsDynamic().Apply(dbEvent);
            }
        }

        public void Add(object evnt)
        {
            EventsWaitingForCommit.Add(evnt);
            this.AsDynamic().Apply(evnt);
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

        public void Apply(NodeAdded e)
        {
            _nodes.Add(_mapper.Map<Node>(e));
        }

        public void Apply(NodeIntoFiberAdded e)
        {
            _nodes.Add(new Node() { Id = e.Id, Latitude = e.Position.Lat, Longitude = e.Position.Lng });
            AddTwoFibersToNewNode(e);
            FixTracesWhichContainedOldFiber(e);
            var fiber = _fibers.FirstOrDefault(f => f.Id == e.FiberId);
            if (fiber != null)
                _fibers.Remove(fiber);
            else _logFile.AppendLine($@"NodeIntoFiberAdded: Fiber {e.FiberId.First6()} not found");
        }

        private void FixTracesWhichContainedOldFiber(NodeIntoFiberAdded e)
        {
            foreach (var trace in _traces)
            {
                int idx;
                while ((idx = Topo.GetFiberIndexInTrace(trace, _fibers.Single(f => f.Id == e.FiberId))) != -1)
                {
                    trace.Nodes.Insert(idx + 1, e.Id); // GPS location добавляется во все трассы
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

        public void Apply(NodeUpdated source)
        {
            var node = _nodes.FirstOrDefault(x => x.Id == source.Id);
            if (node != null)
                _mapper.Map(source, node);
            else _logFile.AppendLine($@"NodeUpdated: Node {source.Id.First6()} not found");
        }

        public void Apply(NodeMoved newLocation) { }

        public void Apply(NodeRemoved e)
        {
            foreach (var trace in _traces.Where(t => t.Nodes.Contains(e.Id)))
            {
                if (e.TraceWithNewFiberForDetourRemovedNode == null ||
                    !e.TraceWithNewFiberForDetourRemovedNode.ContainsKey(trace.Id))
                    _logFile.AppendLine($@"NodeRemoved: No fiber prepared to detour trace {trace.Id}");
                else
                    ExcludeNodeFromTrace(trace, e.TraceWithNewFiberForDetourRemovedNode[trace.Id], e.Id);
            }

            RemoveNodeWithAllHis(e.Id);
        }

        private void ExcludeNodeFromTrace(Trace trace, Guid fiberId, Guid nodeId)
        {
            var idxInTrace = trace.Nodes.IndexOf(nodeId);
            CreateDetourIfAbsent(trace, fiberId, idxInTrace);

            trace.Equipments.RemoveAt(idxInTrace);
            trace.Nodes.RemoveAt(idxInTrace);
        }

        private void RemoveNodeWithAllHis(Guid nodeId)
        {
            _fibers.RemoveAll(f => f.Node1 == nodeId || f.Node2 == nodeId);
            _equipments.RemoveAll(e => e.NodeId == nodeId);
            var node = _nodes.FirstOrDefault(n => n.Id == nodeId);
            if (node != null)
                _nodes.Remove(node);
            else _logFile.AppendLine($@"RemoveNodeWithAllHis: Node {nodeId.First6()} not found");
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
        public void Apply(FiberAdded e)
        {
            _fibers.Add(_mapper.Map<Fiber>(e));
        }
        public void Apply(FiberUpdated source) { }

        public void Apply(FiberRemoved e)
        {
            var fiber = _fibers.FirstOrDefault(f => f.Id == e.Id);
            if (fiber != null)
                _fibers.Remove(fiber);
            else _logFile.AppendLine($@"FiberRemoved: Fiber {e.Id.First6()} not found");
        }
        #endregion

        #region Equipment
        public void Apply(EquipmentIntoNodeAdded e) { _equipments.Add(new Equipment() { Id = e.Id, Type = e.Type, NodeId = e.NodeId }); }

        public void Apply(EquipmentAtGpsLocationAdded e)
        {
            _equipments.Add(new Equipment() { Id = e.Id, Type = e.Type, NodeId = e.NodeId });
            _nodes.Add(new Node() { Id = e.NodeId, Latitude = e.Latitude, Longitude = e.Longitude });
        }

        public void Apply(EquipmentUpdated e) { }

        public void Apply(EquipmentRemoved e) { }
        #endregion

        #region Rtu

        public void Apply(RtuAtGpsLocationAdded e)
        {
            _nodes.Add(new Node() { Id = e.NodeId, Latitude = e.Latitude, Longitude = e.Longitude });
            _rtus.Add(_mapper.Map<Rtu>(e));
        }

        public void Apply(RtuUpdated e) { }

        public void Apply(RtuRemoved e)
        {
            _rtus.RemoveAll(r => r.Id == e.Id);
        }

        public void Apply(MonitoringSettingsChanged e)
        {

        }
        #endregion

        #region Trace
        public void Apply(TraceAdded e)
        {
            _traces.Add(_mapper.Map<Trace>(e));
        }

        public void Apply(TraceCleaned e)
        {
            var trace = _traces.FirstOrDefault(t => t.Id == e.Id);
            if (trace != null)
                _traces.Remove(trace);
        }

        public void Apply(TraceRemoved e)
        {
            var trace = _traces.FirstOrDefault(t => t.Id == e.Id);
            if (trace != null)
                _traces.Remove(trace);
            else _logFile.AppendLine($@"TraceRemoved: Trace {e.Id} not found");
        }

        public void Apply(TraceAttached e) { }

        public void Apply(TraceDetached e) { }

        public void Apply(BaseRefAssigned e) { }
        #endregion

        #region JustEchosOfCmdsSentToRtu
        public void Apply(RtuInitialized e)
        {
            var rtu = _rtus.FirstOrDefault(r => r.Id == e.Id);
            if (rtu != null)
                _mapper.Map(e, rtu);
            else _logFile.AppendLine($@"RtuInitialized: RTU {e.Id.First6()} not found");
        }

        public void Apply(MonitoringStarted e) { }
        public void Apply(MonitoringStopped e) { }


        #endregion
    }
}