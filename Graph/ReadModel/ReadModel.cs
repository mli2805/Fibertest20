using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Iit.Fibertest.Graph.Events;

namespace Iit.Fibertest.Graph
{
    public class ReadModel
    {
        private readonly IMapper _mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingEventToDomainModelProfile>()).CreateMapper();

        public List<Node> Nodes { get; } = new List<Node>();
        public List<Fiber> Fibers { get; } = new List<Fiber>();
        public List<Equipment> Equipments { get; } = new List<Equipment>();
        public List<Rtu> Rtus { get; } = new List<Rtu>();
        public List<Trace> Traces { get; } = new List<Trace>();

        #region Node
        public void Apply(NodeAdded e)
        {
            Node node = _mapper.Map<Node>(e);
            Nodes.Add(node);
        }

        public void Apply(NodeIntoFiberAdded e)
        {
            AddNodeIntoCenterOfFiber(e.Id, e.FiberId);
            // в ReadModel не забыть добавить оборудование
            // здесь не добавляем новое оборудование т.к. пока не придумано зачем оно здесь может понадобиться
            AddTwoFibersToNewNode(e);
            FixTracesWhichContainedOldFiber(e);
            Fibers.Remove(Fibers.Single(f => f.Id == e.FiberId));
        }
        private void FixTracesWhichContainedOldFiber(NodeIntoFiberAdded e)
        {
            foreach (var trace in Traces)
            {
                int idx;
                while ((idx = Topo.GetFiberIndexInTrace(trace, Fibers.Single(f => f.Id == e.FiberId))) != -1)
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
            Fibers.Add(new Fiber() { Id = e.NewFiberId1, Node1 = nodeId1, Node2 = e.Id });
            Fibers.Add(new Fiber() { Id = e.NewFiberId2, Node1 = e.Id, Node2 = nodeId2 });
        }

        private void AddNodeIntoCenterOfFiber(Guid newNodeId, Guid fiberId)
        {
            var center = GetFiberCenter(fiberId);
            Nodes.Add(new Node() { Id = newNodeId, Latitude = center.Latitude, Longitude = center.Longitude });
        }

        private void GetNodesForFiber(Guid fiberId, out Guid nodeId1, out Guid nodeId2)
        {
            nodeId1 = Fibers.Single(f => f.Id == fiberId).Node1;
            nodeId2 = Fibers.Single(f => f.Id == fiberId).Node2;
        }

        private GpsLocation GetFiberCenter(Guid fiberId)
        {
            var fiber = Fibers.Single(f => f.Id == fiberId);
            var node1 = Nodes.Single(n => n.Id == fiber.Node1);
            var node2 = Nodes.Single(n => n.Id == fiber.Node2);
            return new GpsLocation() { Latitude = (node1.Latitude + node2.Latitude) / 2, Longitude = (node1.Longitude + node2.Longitude) / 2 };
        }


        public void Apply(NodeUpdated source)
        {
            Node destination = Nodes.Single(n => n.Id == source.Id);
            _mapper.Map(source, destination);
        }

        public void Apply(NodeMoved newLocation)
        {
            Node oldLocation = Nodes.Single(n => n.Id == newLocation.Id);
            _mapper.Map(newLocation, oldLocation);
        }

        public void Apply(NodeRemoved e)
        {
            foreach (var trace in Traces.Where(t => t.Nodes.Contains(e.Id)))
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
            Fibers.RemoveAll(f => f.Node1 == nodeId || f.Node2 == nodeId);
            Equipments.RemoveAll(e => e.NodeId == nodeId);
            Nodes.Remove(Nodes.Single(n => n.Id == nodeId));
        }

        private void CreateDetourIfAbsent(Trace trace, Guid fiberId, int idxInTrace)
        {
            var nodeBefore = trace.Nodes[idxInTrace - 1];
            var nodeAfter = trace.Nodes[idxInTrace + 1];

            if (!Fibers.Any(f => f.Node1 == nodeBefore && f.Node2 == nodeAfter
                               || f.Node2 == nodeBefore && f.Node1 == nodeAfter))
                Apply(new FiberAdded() { Id = fiberId, Node1 = nodeBefore, Node2 = nodeAfter });
        }
        #endregion

        #region Fiber
        public bool HasFiberBetween(Guid a, Guid b)
        {
            return Fibers.Any(f =>
                f.Node1 == a && f.Node2 == b ||
                f.Node1 == b && f.Node2 == a);
        }
        public bool IsFiberContainedInTraceWithBase(Guid fiberId)
        {
            var tracesWithBase = Traces.Where(t => t.HasBase);
            var fiber = Fibers.Single(f => f.Id == fiberId);
            return tracesWithBase.Any(trace => Topo.GetFiberIndexInTrace(trace, fiber) != -1);
        }

        public void Apply(FiberAdded e)
        {
            Fibers.Add(_mapper.Map<Fiber>(e));
        }

        public void Apply(FiberUpdated source)
        {
            var destination = Fibers.Single(f => f.Id == source.Id);
            _mapper.Map(source, destination);
        }

        public void Apply(FiberRemoved e)
        {
            var fiber = Fibers.Single(f => f.Id == e.Id);
            Fibers.Remove(fiber);
        }
        #endregion

        #region Equipment
        public void Apply(EquipmentAdded e)
        {
            Equipment equipment = _mapper.Map<Equipment>(e);
            Equipments.Add(equipment);
            foreach (var traceId in e.TracesForInsertion)
            {
                var trace = Traces.Single(t => t.Id == traceId);
                var idx = trace.Nodes.IndexOf(e.NodeId);
                trace.Equipments[idx] = e.Id;
            }
        }

        public void Apply(EquipmentAtGpsLocationAdded e)
        {
            Node node = new Node() { Id = e.NodeId, Latitude = e.Latitude, Longitude = e.Longitude };
            Nodes.Add(node);
            Equipment equipment = _mapper.Map<Equipment>(e);
            Equipments.Add(equipment);
        }

        public void Apply(EquipmentUpdated e)
        {
            var equipment = Equipments.Single(eq => eq.Id == e.Id);
            _mapper.Map(e, equipment);
        }

        public void Apply(EquipmentRemoved e)
        {
            var traces = Traces.Where(t => t.Equipments.Contains(e.Id)).ToList();
            if (traces.Any(t => t.HasBase))
                return;
            foreach (var trace in traces)
            {
                var idx = trace.Equipments.IndexOf(e.Id);
                trace.Equipments[idx] = Guid.Empty;
            }
            Equipments.Remove(Equipments.Single(eq => eq.Id == e.Id));
        }
        #endregion

        #region Rtu
        public void Apply(RtuAtGpsLocationAdded e)
        {
            Node node = new Node() {Id = e.NodeId, Latitude = e.Latitude, Longitude = e.Longitude};
            Nodes.Add(node);
            Rtu rtu = _mapper.Map<Rtu>(e);
            Rtus.Add(rtu);
        }

        public void Apply(RtuRemoved e)
        {
            var rtu = Rtus.First(r => r.Id == e.Id);
            var nodeId = rtu.NodeId;
            Rtus.Remove(rtu);
            RemoveNodeWithAllHis(nodeId);
        }
        #endregion

        #region Trace
        public void Apply(TraceAdded e)
        {
            Trace trace = _mapper.Map<Trace>(e);
            Traces.Add(trace);
        }

        public void Apply(TraceAttached e)
        {
            var trace = Traces.Single(t => t.Id == e.TraceId);
            trace.Port = e.Port;
        }

        public void Apply(TraceDetached e)
        {
            var trace = Traces.Single(t => t.Id == e.TraceId);
            trace.Port = -1;
        }

        public void Apply(BaseRefAssigned e)
        {
            // базовая не хранится на клиенте, а получается по запросу
            // в полях трассы хранятся id ее базовых
            // BaseRefs.Add(_mapper.Map<BaseRef>(e));
            var trace = Traces.Single(t => t.Id == e.TraceId);
            if (e.Type == BaseRefType.Precise)
                trace.PreciseId = e.Id;
            else if (e.Type == BaseRefType.Fast)
                trace.FastId = e.Id;
            else if (e.Type == BaseRefType.Additional)
                trace.AdditionalId = e.Id;
        }
        #endregion
    }
}