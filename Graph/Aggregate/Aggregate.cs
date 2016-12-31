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
        private readonly List<Guid> _nodes = new List<Guid>();
        private readonly List<Trace> _traces = new List<Trace>();
        private readonly List<Rtu> _rtus = new List<Rtu>();


        private readonly IMapper _mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingCmdToEventProfile>()).CreateMapper();

        #region Node
        public void When(AddNode cmd)
        {
            Events.Add(_mapper.Map<NodeAdded>(cmd));
            _nodes.Add(cmd.Id);
        }

        public string When(AddNodeIntoFiber cmd)
        {
            if (IsFiberContainedInTraceWithBase(cmd.FiberId))
                return "It's impossible to change trace with base reflectogram";
            Events.Add(_mapper.Map<NodeIntoFiberAdded>(cmd));
            return null;
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

        #endregion

        #region Fiber

        private bool IsFiberContainedInTraceWithBase(Guid fiberId)
        {
            return false;
        }

        public string When(AddFiber cmd)
        {
            if (!_fibersByNodePairs.Add(new NodePairKey(cmd.Node1, cmd.Node2)))
                return "Fiber already exists";

            Events.Add(_mapper.Map<FiberAdded>(cmd));
            return null;
        }

        public string When(AddFiberWithNodes cmd)
        {
            if (!_fibersByNodePairs.Add(new NodePairKey(cmd.Node1, cmd.Node2)))
                return "Fiber already exists";

            Events.Add(_mapper.Map<FiberWithNodesAdded>(cmd));
            return null;
        }

        public string When(RemoveFiber cmd)
        {
            if (IsFiberContainedInTraceWithBase(cmd.Id))
                return "It's impossible to change trace with base reflectogram";
            Events.Add(_mapper.Map<FiberRemoved>(cmd));
            return null;
        }

        #endregion

        public void When(AddEquipment cmd)
        {
            Events.Add(_mapper.Map<EquipmentAdded>(cmd));
        }

        public void When(AddRtuAtGpsLocation cmd)
        {
            Events.Add(_mapper.Map<RtuAddedAtGpsLocation>(cmd));
        }

        public void When(AddTrace cmd)
        {
            Events.Add(_mapper.Map<TraceAdded>(cmd));
        }

        public void When(AddPrecise cmd)
        {
            Events.Add(_mapper.Map<PreciseAdded>(cmd));
        }
    }
}
