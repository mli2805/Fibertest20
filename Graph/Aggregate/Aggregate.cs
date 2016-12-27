using System;
using System.Collections.Generic;
using AutoMapper;
using Iit.Fibertest.Graph.Commands;
using Iit.Fibertest.Graph.Events;

namespace Iit.Fibertest.Graph
{
    public class Aggregate
    {

        public List<object> Events { get; } = new List<object>();
        private readonly HashSet<NodePairKey> FibersByNodePairs = new HashSet<NodePairKey>();
        private readonly HashSet<string> NodeTitles = new HashSet<string>();

        private readonly IMapper _mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<CommandAndEventsProfile>()).CreateMapper();

        public void When(AddNode cmd)
        {
            Events.Add(new NodeAdded
            {
                Id = cmd.Id,
                Latitude = cmd.Latitude,
                Longitude = cmd.Longitude,
            });
        }

        public void When(AddFiber cmd)
        {
            if (!FibersByNodePairs.Add(new NodePairKey(cmd.Node1, cmd.Node2)))
                throw new Exception("already exists");

            Events.Add(new FiberAdded
            {
                Id = cmd.Id,
                Node1 = cmd.Node1,
                Node2 = cmd.Node2,
            });
        }

        public void When(AddEquipment cmd)
        {
            Events.Add(_mapper.Map<EquipmentAdded>(cmd));
        }

        public void When(RemoveNode cmd)
        {
            Events.Add(new NodeRemoved
            {
                Id = cmd.Id,
            });
        }
        public string When(UpdateNode cmd)
        {
            if (!NodeTitles.Add(cmd.Title))
                return "node title already exists";
            Events.Add(_mapper.Map<NodeUpdated>(cmd));
            return null;
        }
    }
}
