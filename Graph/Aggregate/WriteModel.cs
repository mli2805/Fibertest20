using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Iit.Fibertest.Graph.Events;
using PrivateReflectionUsingDynamic;

namespace Iit.Fibertest.Graph
{
    public class WriteModel
    {
        public Db Db { get; }
        public List<object> Events { get; } = new List<object>();
        private readonly IMapper _mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingEventToDomainModelProfile>()).CreateMapper();

        private readonly List<Node> _nodes = new List<Node>();
        private readonly List<Fiber> _fibers = new List<Fiber>();
        private readonly List<Equipment> _equipments = new List<Equipment>();
        private readonly List<Trace> _traces = new List<Trace>();
        private readonly List<Rtu> _rtus = new List<Rtu>();

        public WriteModel(Db db)
        {
            Db = db;
        }

        public void AddAndCommit(object evnt)
        {
            Add(evnt);
            Commit();
        }

        public void Add(object evnt)
        {
            Events.Add(evnt);
            this.AsDynamic().Apply(evnt);
        }

        public void Commit()
        {
            foreach (var @event in Events)
                Db.Add(@event);
            Events.Clear();
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
            return _traces.Single(t => t.Id == traceId);
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
            _nodes.Add(new Node() { Id = e.Id, Latitude = e.Position.Latitude, Longitude = e.Position.Longitude });
            AddTwoFibersToNewNode(e);
            FixTracesWhichContainedOldFiber(e);
            _fibers.Remove(_fibers.Single(f => f.Id == e.FiberId));
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
            Guid nodeId1 = _fibers.Single(f => f.Id == e.FiberId).Node1;
            Guid nodeId2 = _fibers.Single(f => f.Id == e.FiberId).Node2;

            _fibers.Add(new Fiber() { Id = e.NewFiberId1, Node1 = nodeId1, Node2 = e.Id });
            _fibers.Add(new Fiber() { Id = e.NewFiberId2, Node1 = e.Id, Node2 = nodeId2 });
        }

        public void Apply(NodeUpdated source)
        {
            _mapper.Map(source, _nodes.Single(x => x.Id == source.Id));
        }

        public void Apply(NodeMoved newLocation) { }

        public void Apply(NodeRemoved e)
        {
            foreach (var trace in _traces.Where(t => t.Nodes.Contains(e.Id)))
                ExcludeNodeFromTrace(trace, e.TraceFiberPairForDetour[trace.Id], e.Id);

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
            _nodes.Remove(_nodes.Single(n => n.Id == nodeId));
        }

        private void CreateDetourIfAbsent(Trace trace, Guid fiberId, int idxInTrace)
        {
            var nodeBefore = trace.Nodes[idxInTrace - 1];
            var nodeAfter = trace.Nodes[idxInTrace + 1];

            if (!_fibers.Any(f=>f.Node1 == nodeBefore && f.Node2 == nodeAfter
                               || f.Node2 == nodeBefore && f.Node1 == nodeAfter))
                Apply(new FiberAdded() { Id =  fiberId, Node1 = nodeBefore, Node2 = nodeAfter });
        }

        public bool IsFiberContainedInAnyTraceWithBase(Guid fiberId)
        {
            var tracesWithBase = _traces.Where(t => t.HasBase);
            var fiber = _fibers.Single(f => f.Id == fiberId);
            foreach (var trace in tracesWithBase)
            {
                if (Topo.GetFiberIndexInTrace(trace, fiber) != -1)
                    return true;
            }
            return false;
        }
        public bool IsNodeContainedInAnyTraceWithBase(Guid nodeId)
        {
            return _traces.Any(t => t.HasBase && t.Nodes.Contains(nodeId));
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

        public void Apply(FiberRemoved e) { }
        #endregion

        #region Equipment
        public void Apply(EquipmentIntoNodeAdded e) { _equipments.Add(new Equipment() {Id = e.Id, Type = e.Type, NodeId = e.NodeId}); }

        public void Apply(EquipmentAtGpsLocationAdded e)
        {
            _equipments.Add(new Equipment() { Id = e.Id, Type = e.Type, NodeId = e.NodeId });
            _nodes.Add(new Node() { Id = e.NodeId, Latitude = e.Latitude, Longitude = e.Longitude });
        }

        public Equipment GetEquipment(Guid id) { return _equipments.SingleOrDefault(e=>e.Id == id); }
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
        public void Apply(RtuRemoved e) { }
        #endregion

        #region Trace

        public void Apply(TraceAdded e)
        {
            _traces.Add(_mapper.Map<Trace>(e));

        }

        public void Apply(TraceAttached e) { }

        public void Apply(TraceDetached e) { }

        public void Apply(BaseRefAssigned e) { }
        #endregion

    }
}