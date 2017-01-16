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
//        private readonly List<Equipment> _equipments = new List<Equipment>();
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

        public bool HasNodeWithTitle(string title)
        {
            return _nodes.Any(n => n.Title ==  title);
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
            return _rtus.FirstOrDefault(r => r.Id == id) ;
        }

        #region Node

        public void Apply(NodeAdded e)
        {
            _nodes.Add(_mapper.Map<Node>(e));
        }

        public void Apply(NodeIntoFiberAdded e)
        {
            AddNodeIntoCenterOfFiber(e.Id, e.FiberId);
            // в ReadModel не забыть добавить оборудование
            // здесь не добавляем новое оборудование т.к. пока не придумано зачем оно здесь может понадобиться
            AddTwoFibersToNewNode(e);
            FixTracesWhichContainedOldFiber(e);
        }
        private void FixTracesWhichContainedOldFiber(NodeIntoFiberAdded e)
        {
            foreach (var trace in _traces)
            {
                int idx;
                while ((idx = Topo.GetFiberIndexInTrace(trace, _fibers.Single(f => f.Id == e.FiberId))) != -1)
                {
                    trace.Nodes.Insert(idx + 1, e.Id); // GPS location добавляется во все трассы
                    // а оборудование только в те, которые выбрал пользователь
                    trace.Equipments.Insert(idx + 1,
                        e.TracesConsumingEquipment.Contains(trace.Id) ? e.EquipmentId : Guid.Empty);
                }
            }
        }
        private void AddTwoFibersToNewNode(NodeIntoFiberAdded e)
        {
            Guid nodeId1, nodeId2;
            GetNodesForFiber(e.FiberId, out nodeId1, out nodeId2);
            _fibers.Add(new Fiber() { Id = e.NewFiberId1, Node1 = nodeId1, Node2 = e.Id });
            _fibers.Add(new Fiber() { Id = e.NewFiberId2, Node1 = e.Id, Node2 = nodeId2 });
        }

        private void AddNodeIntoCenterOfFiber(Guid newNodeId, Guid fiberId)
        {
            var center = GetFiberCenter(fiberId);
            _nodes.Add(new Node(){ Id = newNodeId, Latitude = center.Latitude, Longitude = center.Longitude });
        }


        public void Apply(NodeUpdated source)
        {
            _mapper.Map(source, _nodes.Single(x => x.Id == source.Id));

        }

        public void Apply(NodeMoved newLocation) { }

        public void Apply(NodeRemoved e)
        {
            
        }


        #endregion

        #region Fiber
        public bool IsFiberContainedInTraceWithBase(Guid fiberId)
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

        private GpsLocation GetFiberCenter(Guid fiberId)
        {
            var fiber = _fibers.Single(f => f.Id == fiberId);
            var node1 = _nodes.Single(n => n.Id == fiber.Node1);
            var node2 = _nodes.Single(n => n.Id == fiber.Node2);
            return new GpsLocation() { Latitude = (node1.Latitude + node2.Latitude) / 2, Longitude = (node1.Longitude + node2.Longitude) / 2 };
        }

        private void GetNodesForFiber(Guid fiberId, out Guid nodeId1, out Guid nodeId2)
        {
            nodeId1 = _fibers.Single(f => f.Id == fiberId).Node1;
            nodeId2 = _fibers.Single(f => f.Id == fiberId).Node2;
        }

        

        public void Apply(FiberAdded e)
        {
            _fibers.Add(_mapper.Map<Fiber>(e));

        }
        public void Apply(FiberUpdated source) { }

        public void Apply(FiberRemoved e) { }
        #endregion

        #region Equipment
        public void Apply(EquipmentAdded e) { }

        public void Apply(EquipmentAtGpsLocationAdded e)
        {
       //  TODO:   _nodes.Add(_mapper.Map<Node>(e));
        }

        public void Apply(EquipmentUpdated e) { }

        public void Apply(EquipmentRemoved e) { }
        #endregion

        #region Rtu

        public void Apply(RtuAtGpsLocationAdded e)
        {
            _rtus.Add(_mapper.Map<Rtu>(e));
        }

        public void Apply(RtuRemoved e) { }
        #endregion

        #region Trace

        public void Apply(TraceAdded e)
        {
            _traces.Add(_mapper.Map<Trace>(e));

        }

        public void Apply(TraceAttached e) { }

        public void Apply(TraceDetached e) { }

        public void Apply(BaseRefAssigned e){}
        #endregion

    }
}