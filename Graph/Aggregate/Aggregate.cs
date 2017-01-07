using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Iit.Fibertest.Graph.Commands;
using Iit.Fibertest.Graph.Events;
using PrivateReflectionUsingDynamic;

namespace Iit.Fibertest.Graph
{
    public class Db
    {
        public List<object> Events { get; } = new List<object>();

        public void Add(object e)
        {
            Events.Add(e);
        }
    }

    public class ClientPoller
    {
        private readonly Db _db;
        public List<object> ReadModels { get; } 
        public int CurrentEventNumber { get; private set; }

        public ClientPoller(Db db, List<object> readModels)
        {
            _db = db;
            ReadModels = readModels;
        }

        public void Tick()
        {
            foreach (var e in _db.Events.Skip(CurrentEventNumber))
                foreach (var m in ReadModels)
                    m.AsDynamic().Apply(e);
            CurrentEventNumber = _db.Events.Count;
        }
    }
    public class Aggregate
    {
        public Db Db { get; } = new Db();

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
            Db.Add(_mapper.Map<NodeAdded>(cmd));
            _nodes.Add(cmd.Id);
        }

        public string When(AddNodeIntoFiber cmd)
        {
            if (IsFiberContainedInTraceWithBase(cmd.FiberId))
                return "It's impossible to change trace with base reflectogram";
            Db.Add(_mapper.Map<NodeIntoFiberAdded>(cmd));
            return null;
        }

        public string When(UpdateNode cmd)
        {
            if (!IsNodeTitleValid(cmd.Title))
                return "node title already exists";
            Db.Add(_mapper.Map<NodeUpdated>(cmd));
            return null;
        }

        private bool IsNodeTitleValid(string title)
        {
            return _nodeTitles.Add(title);
        }
        public void When(MoveNode cmd)
        {
            Db.Add(_mapper.Map<NodeMoved>(cmd));
        }

        public void When(RemoveNode cmd)
        {
            Db.Add(new NodeRemoved
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

            Db.Add(_mapper.Map<FiberAdded>(cmd));
            return null;
        }

        public string When(AddFiberWithNodes cmd)
        {
            if (!_fibersByNodePairs.Add(new NodePairKey(cmd.Node1, cmd.Node2)))
                return "Fiber already exists";

            Db.Add(_mapper.Map<FiberWithNodesAdded>(cmd));
            return null;
        }

        public string When(RemoveFiber cmd)
        {
            if (IsFiberContainedInTraceWithBase(cmd.Id))
                return "It's impossible to change trace with base reflectogram";
            Db.Add(_mapper.Map<FiberRemoved>(cmd));
            return null;
        }

        #endregion

        public void When(AddEquipment cmd)
        {
            Db.Add(_mapper.Map<EquipmentAdded>(cmd));
        }

        public void When(AddRtuAtGpsLocation cmd)
        {
            Db.Add(_mapper.Map<RtuAddedAtGpsLocation>(cmd));
        }

        #region MyRegion
        public void When(AddTrace cmd)
        {
            Db.Add(_mapper.Map<TraceAdded>(cmd));
        }

        public void When(AttachTrace cmd)
        {
            Db.Add(_mapper.Map<TraceAttached>(cmd));
        }

        public void When(DetachTrace cmd)
        {
            Db.Add(_mapper.Map<TraceDetached>(cmd));
        }

        public void When(AssignBaseRef cmd)
        {
            Db.Add(_mapper.Map<BaseRefAssigned>(cmd));
        }
        #endregion
    }
}
