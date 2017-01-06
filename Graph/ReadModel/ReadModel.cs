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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="trace">Trace could contain the same fiber more then once</param>
        /// <param name="fiber">The object to locate</param>
        /// <returns>The zero-based index of the first occurence of the fiber within the entire list of fibers in trace if found; otherwise, -1</returns>
        private int GetFiberIndexInTrace(Trace trace, Fiber fiber)
        {
            var idxInTrace1 = trace.Nodes.IndexOf(fiber.Node1);
            if (idxInTrace1 == -1)
                return -1;
            var idxInTrace2 = trace.Nodes.IndexOf(fiber.Node2);
            if (idxInTrace2 == -1)
                return -1;
            if (idxInTrace2 - idxInTrace1 == 1)
                return idxInTrace1;
            if (idxInTrace1 - idxInTrace2 == 1)
                return idxInTrace2;
            return -1;
        }
        public void Apply(NodeIntoFiberAdded e)
        {
            Node node = _mapper.Map<Node>(e);

            var fiber = Fibers.Single(f => f.Id == e.FiberId);
            var node1 = Nodes.Single(n => n.Id == fiber.Node1);
            var node2 = Nodes.Single(n => n.Id == fiber.Node2);

            node.Latitude = Math.Abs(node1.Latitude - node2.Latitude) / 2;
            node.Longitude = Math.Abs(node1.Longitude - node2.Longitude) / 2;
            Nodes.Add(node);

            var leftFiber = new Fiber() {Id = Guid.NewGuid(), Node1 = node1.Id, Node2 = node.Id};
            Fibers.Add(leftFiber);
            var rightFiber = new Fiber() {Id = Guid.NewGuid(), Node1 = node.Id, Node2 = node2.Id};
            Fibers.Add(rightFiber);

            foreach (var trace in Traces)
            {
                int idx;
                while ((idx = GetFiberIndexInTrace(trace, fiber)) != -1)
                {
                    trace.Nodes.Insert(idx+1, node.Id);
                }
            }

            Fibers.Remove(fiber);
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
            Node node = Nodes.Single(n=>n.Id == e.Id);
            Nodes.Remove(node);
        }

        #endregion

        #region Fiber
        public void Apply(FiberAdded e)
        {
            Fiber fiber = _mapper.Map<Fiber>(e);
            Fibers.Add(fiber);
        }

        public void Apply(FiberWithNodesAdded e)
        {
            Fiber fiber = new Fiber() { Id = Guid.NewGuid(), Node1 = e.Node1 };

            for (int i = 0; i < e.IntermediateNodesCount; i++)
            {
                Node node = new Node() {Id = Guid.NewGuid()};
                Nodes.Add(node);

                Equipment equipment = new Equipment()
                {
                    Id = Guid.NewGuid(), NodeId = node.Id, Type = e.EquipmentInIntermediateNodesType,
                };
                Equipments.Add(equipment);

                fiber.Node2 = node.Id;
                Fibers.Add(fiber);
                fiber = new Fiber() {Id = Guid.NewGuid(), Node1 = node.Id};
            }

            fiber.Node2 = e.Node2;
            Fibers.Add(fiber);
        }

        public void Apply(FiberRemoved e)
        {
            var fiber = Fibers.Single(f => f.Id == e.Id);
            Fibers.Remove(fiber);
        }
        #endregion

        public void Apply(EquipmentAdded e)
        {
            Equipment equipment = _mapper.Map<Equipment>(e);
            Equipments.Add(equipment);
        }

        public void Apply(RtuAddedAtGpsLocation e)
        {
            Node node = new Node() {Id = e.NodeId};
            Nodes.Add(node);
            Rtu rtu = _mapper.Map<Rtu>(e);
            Rtus.Add(rtu);
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

        public void Apply(TraceAdded e)
        {
            Trace trace = _mapper.Map<Trace>(e);
            Traces.Add(trace);
        }


        public void Apply(BaseRefAssigned e)
        {
            Trace trace = Traces.Single(t => t.Id == e.TraceId);
            trace.PreciseId = e.Id;
        }
    }
}