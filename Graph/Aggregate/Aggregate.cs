using System;
using System.Linq;
using AutoMapper;
using Iit.Fibertest.Graph.Commands;
using Iit.Fibertest.Graph.Events;

namespace Iit.Fibertest.Graph
{
    public class Aggregate
    {
        public WriteModel WriteModel { get; } = new WriteModel(new Db());

        private readonly IMapper _mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingCmdToEventProfile>()).CreateMapper();


        #region Node
        public void When(AddNode cmd)
        {
            WriteModel.AddAndCommit(_mapper.Map<NodeAdded>(cmd));
        }

        public string When(AddNodeIntoFiber cmd)
        {
            if (WriteModel.IsFiberContainedInAnyTraceWithBase(cmd.FiberId))
                return "It's impossible to change trace with base reflectogram";

            WriteModel.AddAndCommit(_mapper.Map<NodeIntoFiberAdded>(cmd));
            return null;
        }

        public string When(UpdateNode cmd)
        {
            // TODO: test when title doesn't change
            // TODO: test when title changes and then old title reused
            if (WriteModel.HasAnotherNodeWithTitle(cmd.Title, cmd.Id))
                return "node title already exists";
            WriteModel.AddAndCommit(_mapper.Map<NodeUpdated>(cmd));
            return null;
        }


        public void When(MoveNode cmd)
        {
            WriteModel.AddAndCommit(_mapper.Map<NodeMoved>(cmd));
        }

        public string When(RemoveNode cmd)
        {
            if (WriteModel.IsNodeLastForAnyTrace(cmd.Id))
                return "It's prohibited to remove last node from trace";
            if (WriteModel.IsNodeContainedInAnyTraceWithBase(cmd.Id))
                return "It's impossible to change trace with base reflectogram";

            WriteModel.AddAndCommit(_mapper.Map<NodeRemoved>(cmd));
            return null;
        }
        #endregion

        #region Fiber

        public string When(AddFiber cmd)
        {
            if (WriteModel.HasFiberBetween(cmd.Node1, cmd.Node2))
                return "Fiber already exists";

            WriteModel.AddAndCommit(_mapper.Map<FiberAdded>(cmd));
            return null;
        }

        public string When(AddFiberWithNodes cmd)
        {
            if (WriteModel.HasFiberBetween(cmd.Node1, cmd.Node2))
                return "Fiber already exists";
            
            if (cmd.AddNodes != null)
                foreach (var cmdAddNode in cmd.AddNodes)
                    WriteModel.Add(_mapper.Map<NodeAdded>(cmdAddNode));
            else
                foreach (var cmdAddEquipmentAtGpsLocation in cmd.AddEquipments)
                    WriteModel.Add(_mapper.Map<EquipmentAtGpsLocationAdded>(cmdAddEquipmentAtGpsLocation));

            foreach (var cmdAddFiber in cmd.AddFibers)
                WriteModel.Add(_mapper.Map<FiberAdded>(cmdAddFiber));

            WriteModel.Commit();
            return null;
        }

        public void When(UpdateFiber cmd)
        {
            WriteModel.AddAndCommit(_mapper.Map<FiberUpdated>(cmd));
        }
        public string When(RemoveFiber cmd)
        {
            if (WriteModel.IsFiberContainedInAnyTraceWithBase(cmd.Id))
                return "It's impossible to change trace with base reflectogram";
            WriteModel.AddAndCommit(_mapper.Map<FiberRemoved>(cmd));
            return null;
        }

        #endregion

        #region Equipment
        public string When(AddEquipment cmd)
        {
            foreach (var traceId in cmd.TracesForInsertion)
            {
                var trace = WriteModel.GetTrace(traceId);
                if (trace.HasBase)
                    return "Base ref is set for trace";

                var idx = trace.Nodes.IndexOf(cmd.NodeId);
                if (trace.Equipments[idx] != Guid.Empty)
                    return "Node contains equipment for trace already";
            }
            WriteModel.AddAndCommit(_mapper.Map<EquipmentAdded>(cmd));

            foreach (var traceId in cmd.TracesForInsertion)
            {
                var trace = WriteModel.GetTrace(traceId);
                var idx = trace.Nodes.IndexOf(cmd.NodeId);
                trace.Equipments[idx] = cmd.Id;
            }
            return null;
        }

        public void When(AddEquipmentAtGpsLocation cmd)
        {
            WriteModel.AddAndCommit(_mapper.Map<EquipmentAtGpsLocationAdded>(cmd));
        }

        public string When(UpdateEquipment cmd)
        {
//            if (WriteModel.GetEquipment(cmd.Id) == null)
//                return "Somebody removed this equipment while you updated it";
            WriteModel.AddAndCommit(_mapper.Map<EquipmentUpdated>(cmd));
            return null;
        }
        public void When(RemoveEquipment cmd)
        {
            WriteModel.AddAndCommit(_mapper.Map<EquipmentRemoved>(cmd));
        }
        #endregion

        #region Rtu
        public void When(AddRtuAtGpsLocation cmd)
        {
            WriteModel.AddAndCommit(_mapper.Map<RtuAtGpsLocationAdded>(cmd));
        }

        public void When(RemoveRtu cmd)
        {
            WriteModel.AddAndCommit(_mapper.Map<RtuRemoved>(cmd));
        }
        #endregion

        #region Trace
        public string When(AddTrace cmd)
        {
            var rtu = WriteModel.GetRtu(cmd.RtuId);
            if (rtu == null)
                return "RTU is not found";
            if (cmd.Equipments[0] != cmd.RtuId)
                return "Trace should start from RTU";
            if (cmd.Nodes.Count != cmd.Equipments.Count)
                return "Equipments count in trace should match nodes count";
            if (cmd.Equipments.Last() == Guid.Empty)
                return "Last node of trace must contain equipment";

            WriteModel.AddAndCommit(_mapper.Map<TraceAdded>(cmd));
            return null;
            //_traces.Add(_mapper2.Map<Trace>(cmd));
        }

        public void When(AttachTrace cmd)
        {
            WriteModel.AddAndCommit(_mapper.Map<TraceAttached>(cmd));
        }

        public void When(DetachTrace cmd)
        {
            WriteModel.AddAndCommit(_mapper.Map<TraceDetached>(cmd));
        }

        public void When(AssignBaseRef cmd)
        {
            WriteModel.AddAndCommit(_mapper.Map<BaseRefAssigned>(cmd));
            var trace = WriteModel.GetTrace(cmd.TraceId);
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
