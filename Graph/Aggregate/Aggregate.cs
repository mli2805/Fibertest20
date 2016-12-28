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
        private readonly HashSet<NodePairKey> _fibersByNodePairs = new HashSet<NodePairKey>();
        private readonly HashSet<string> _nodeTitles = new HashSet<string>();

        private readonly IMapper _mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingCmdToEventProfile>()).CreateMapper();

        public void When(AddNode cmd)
        {
            Events.Add(_mapper.Map<NodeAdded>(cmd));
        }

        public void When(AddFiber cmd)
        {
            if (!_fibersByNodePairs.Add(new NodePairKey(cmd.Node1, cmd.Node2)))
                throw new Exception("already exists");

            Events.Add(_mapper.Map<FiberAdded>(cmd));
        }

        public void When(AddFiberWithNodes cmd)
        {
            if (!_fibersByNodePairs.Add(new NodePairKey(cmd.Node1, cmd.Node2)))
                throw new Exception("already exists");

            Events.Add(_mapper.Map<FiberWithNodesAdded>(cmd));
        }


        public void When(AddEquipment cmd)
        {
            Events.Add(_mapper.Map<EquipmentAdded>(cmd));
        }

        public void When(MoveNode cmd)
        {
            Events.Add(_mapper.Map<NodeMoved>(cmd));
        }

        public void When(RemoveNode cmd)
        {
            Events.Add(new NodeRemoved
            {
                Id = cmd.Id,
            });
        }

        public void When(AddRtuAtGpsLocation cmd)
        {
            Events.Add(_mapper.Map<RtuAddedAtGpsLocation>(cmd));
        }
        public string When(UpdateNode cmd)
        {
            if (!IsNodeTitleValid(cmd.Title))
                return "node title already exists";
            Events.Add(_mapper.Map<NodeUpdated>(cmd));
            return null;
        }

        private bool IsNodeTitleValid(string title)
        {
            return _nodeTitles.Add(title);
        }
    }
}
