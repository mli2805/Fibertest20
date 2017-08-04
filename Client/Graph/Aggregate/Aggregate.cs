using System;
using System.Linq;
using AutoMapper;
using Dto;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Graph
{
    public class Aggregate
    {

        public WriteModel WriteModel { get; }

        private readonly IMapper _mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingCmdToEventProfile>()).CreateMapper();

        public Aggregate(WriteModel writeModel)
        {
            WriteModel = writeModel;
        }

        #region Node
        public void When(AddNode cmd)
        {
            WriteModel.AddAndCommit(_mapper.Map<NodeAdded>(cmd));
        }

        public string When(AddNodeIntoFiber cmd)
        {
            if (WriteModel.IsFiberContainedInAnyTraceWithBase(cmd.FiberId))
                return Resources.SID_It_s_impossible_to_change_trace_with_base_reflectogram;

            WriteModel.AddAndCommit(_mapper.Map<NodeIntoFiberAdded>(cmd));
            return null;
        }

        public string When(UpdateNode cmd)
        {
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
                return Resources.SID_It_s_prohibited_to_remove_last_node_from_trace;
            if (WriteModel.IsNodeContainedInAnyTraceWithBase(cmd.Id))
                return Resources.SID_It_s_impossible_to_change_trace_with_base_reflectogram;

            WriteModel.AddAndCommit(_mapper.Map<NodeRemoved>(cmd));
            return null;
        }
        #endregion

        #region Fiber

        public string When(AddFiber cmd)
        {
            if (WriteModel.HasFiberBetween(cmd.Node1, cmd.Node2))
                return Resources.SID_Section_already_exists;

            WriteModel.AddAndCommit(_mapper.Map<FiberAdded>(cmd));
            return null;
        }

        public string When(AddFiberWithNodes cmd)
        {
            if (WriteModel.HasFiberBetween(cmd.Node1, cmd.Node2))
                return Resources.SID_Section_already_exists;

            if (cmd.AddNodes.Count > 0)
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
                return Resources.SID_It_s_impossible_to_change_trace_with_base_reflectogram;
            WriteModel.AddAndCommit(_mapper.Map<FiberRemoved>(cmd));
            return null;
        }

        #endregion

        #region Equipment
        public string When(AddEquipmentIntoNode cmd)
        {
            foreach (var traceId in cmd.TracesForInsertion)
            {
                var trace = WriteModel.GetTrace(traceId);
                if (trace.HasBase)
                    return Resources.SID_Base_ref_is_set_for_trace;
            }
            WriteModel.AddAndCommit(_mapper.Map<EquipmentIntoNodeAdded>(cmd));

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
            //            if (WriteModel.GetEquipment(cmd.RtuId) == null)
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

        public void When(InitializeRtu cmd)
        {
            var evnt = _mapper.Map<RtuInitialized>(cmd);
            WriteModel.AddAndCommit(evnt);
        }

        public void When(UpdateRtu cmd)
        {
            WriteModel.AddAndCommit(_mapper.Map<RtuUpdated>(cmd));
        }

        public void When(RemoveRtu cmd)
        {
            WriteModel.AddAndCommit(_mapper.Map<RtuRemoved>(cmd));
        }
        public void When(AttachOtau cmd)
        {
            WriteModel.AddAndCommit(_mapper.Map<OtauAttached>(cmd));
        }
        public void When(DetachOtau cmd)
        {
            WriteModel.AddAndCommit(_mapper.Map<OtauDetached>(cmd));
        }
        #endregion

        #region Trace
        public string When(AddTrace cmd)
        {
            var rtu = WriteModel.GetRtu(cmd.RtuId);
            if (rtu == null)
                return Resources.SID_RTU_is_not_found;
            if (cmd.Equipments[0] != cmd.RtuId)
                return Resources.SID_Trace_should_start_from_RTU;
            if (cmd.Nodes.Count != cmd.Equipments.Count)
                return Resources.SID_Equipments_count_in_trace_should_match_nodes_count;
            if (cmd.Equipments.Last() == Guid.Empty)
                return Resources.SID_Last_node_of_trace_must_contain_some_equipment;

            WriteModel.AddAndCommit(_mapper.Map<TraceAdded>(cmd));
            return null;
            //_traces.Add(_mapper2.Map<Trace>(cmd));
        }

        public void When(UpdateTrace cmd)
        {
            WriteModel.AddAndCommit(_mapper.Map<TraceUpdated>(cmd));
        }

        public void When(CleanTrace cmd)
        {
            WriteModel.AddAndCommit(_mapper.Map<TraceCleaned>(cmd));
        }

        public void When(RemoveTrace cmd)
        {
            WriteModel.AddAndCommit(_mapper.Map<TraceRemoved>(cmd));
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
            if (cmd.Ids.ContainsKey(BaseRefType.Precise))
                trace.PreciseId = cmd.Ids[BaseRefType.Precise];
            if (cmd.Ids.ContainsKey(BaseRefType.Fast))
                trace.FastId = cmd.Ids[BaseRefType.Fast];
            if (cmd.Ids.ContainsKey(BaseRefType.Additional))
                trace.AdditionalId = cmd.Ids[BaseRefType.Additional];
        }
        #endregion
    }
}
