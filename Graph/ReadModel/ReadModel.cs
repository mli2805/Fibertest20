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

        public void Apply(NodeAdded e)
        {
            Node node = _mapper.Map<Node>(e);
            Nodes.Add(node);
        }

        private Tuple<double, double> GetFiberCenter(Guid fiberId)
        {
            var fiber = Fibers.Single(f => f.Id == fiberId);
            var node1 = Nodes.Single(n => n.Id == fiber.Node1);
            var node2 = Nodes.Single(n => n.Id == fiber.Node2);
            var latitude = Math.Abs(node1.Latitude - node2.Latitude) / 2;
            var longitude = Math.Abs(node1.Longitude - node2.Longitude) / 2;
            return new Tuple<double, double>(latitude, longitude);
        }
        public void Apply(NodeIntoFiberAdded e)
        {
            Node node = _mapper.Map<Node>(e);
            var coors = GetFiberCenter(e.FiberId);
            node.Latitude = coors.Item1;
            node.Longitude = coors.Item2;
            Nodes.Add(node);
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
    }
}