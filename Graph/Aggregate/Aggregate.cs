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
        private readonly List<Fiber> _fibers = new List<Fiber>();
        private readonly List<Trace> _traces = new List<Trace>();
        private readonly List<Rtu> _rtus = new List<Rtu>();


        private readonly IMapper _mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingCmdToEventProfile>()).CreateMapper();
        private readonly IMapper _mapper2 = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingCmdToDomainModelProfile>()).CreateMapper();


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
            Db.Add(_mapper.Map<NodeRemoved>(cmd));
        }

        #endregion

        #region Fiber

        private bool IsFiberContainedInTraceWithBase(Guid fiberId)
        {
            var tracesWithBase = _traces.Where(t => t.HasBase);
            var fiber = _fibers.Single(f => f.Id == fiberId);
            foreach (var trace in tracesWithBase)
            {
                var idx = trace.Nodes.IndexOf(fiber.Node1);
                if (idx == -1)
                    continue;
                if ((idx == 0 && trace.Nodes[1] == fiber.Node2)
                    || (idx == trace.Nodes.Count-1 && trace.Nodes[idx-1] == fiber.Node2)
                    || (trace.Nodes[idx-1] == fiber.Node2)
                    || (trace.Nodes[idx+1] == fiber.Node2) )
                return true;
            }
            return false;
        }

        public string When(AddFiber cmd)
        {
            if (!_fibersByNodePairs.Add(new NodePairKey(cmd.Node1, cmd.Node2)))
                return "Fiber already exists";

            Db.Add(_mapper.Map<FiberAdded>(cmd));
            _fibers.Add(_mapper2.Map<Fiber>(cmd));
            return null;
        }

        public string When(AddFiberWithNodes cmd)
        {
            if (!_fibersByNodePairs.Add(new NodePairKey(cmd.Node1, cmd.Node2)))
                return "Fiber already exists";

            Db.Add(_mapper.Map<FiberWithNodesAdded>(cmd));
            return null;
        }

        public void When(UpdateFiber cmd)
        {
            Db.Add(_mapper.Map<FiberUpdated>(cmd));
        }
        public string When(RemoveFiber cmd)
        {
            if (IsFiberContainedInTraceWithBase(cmd.Id))
                return "It's impossible to change trace with base reflectogram";
            Db.Add(_mapper.Map<FiberRemoved>(cmd));
            return null;
        }

        #endregion

        #region Equipment
        public string When(AddEquipment cmd)
        {
            foreach (var traceId in cmd.TracesForInsertion)
            {
                var trace = _traces.Single(t => t.Id == traceId);
                if (trace.HasBase)
                    return "Base ref is set for trace";

                var idx = trace.Nodes.IndexOf(cmd.NodeId);
                if (trace.Equipments[idx] != Guid.Empty)
                    return "Node contains equipment for trace already";
            }
            Db.Add(_mapper.Map<EquipmentAdded>(cmd));

            foreach (var traceId in cmd.TracesForInsertion)
            {
                var trace = _traces.Single(t => t.Id == traceId);
                var idx = trace.Nodes.IndexOf(cmd.NodeId);
                trace.Equipments[idx] = cmd.Id;
            }
            return null;
        }

        public void When(AddEquipmentAtGpsLocation cmd)
        {
            Db.Add(_mapper.Map<EquipmentAtGpsLocationAdded>(cmd));
        }

        public void When(UpdateEquipment cmd)
        {
            Db.Add(_mapper.Map<EquipmentUpdated>(cmd));
        }
        public void When(RemoveEquipment cmd)
        {
            Db.Add(_mapper.Map<EquipmentRemoved>(cmd));
        }
        #endregion

        #region Rtu
        public void When(AddRtuAtGpsLocation cmd)
        {
            Db.Add(_mapper.Map<RtuAtGpsLocationAdded>(cmd));
            _rtus.Add(_mapper2.Map<Rtu>(cmd));
        }

        public void When(RemoveRtu cmd)
        {
            Db.Add(_mapper.Map<RtuRemoved>(cmd));
        }
        #endregion

        #region Trace
        public void When(AddTrace cmd)
        {
            if ((_rtus.FirstOrDefault(r=>r.Id == cmd.RtuId) == null)
                || (cmd.Equipments[0] != cmd.RtuId)
                || (cmd.Nodes.Count != cmd.Equipments.Count)
                || (cmd.Equipments.Last() == Guid.Empty))
                return;
            Db.Add(_mapper.Map<TraceAdded>(cmd));
            _traces.Add(_mapper2.Map<Trace>(cmd));
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
            var trace = _traces.Single(t => t.Id == cmd.TraceId);
            if (cmd.Type == BaseRefType.Precise)
                trace.PreciseId = cmd.Id;
            else if (cmd.Type == BaseRefType.Fast)
                trace.FastId = cmd.Id;
            else if (cmd.Type == BaseRefType.Additional)
                trace.AdditionalId = cmd.Id;
        }
        #endregion
    }
}
