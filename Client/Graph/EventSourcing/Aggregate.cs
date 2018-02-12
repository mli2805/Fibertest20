using System;
using System.Linq;
using AutoMapper;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Graph
{
    public class Aggregate
    {
        private readonly IMyLog _logFile;

        public WriteModel WriteModel { get; }

        private readonly IMapper _mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingCmdToEventProfile>()).CreateMapper();

        public Aggregate(IMyLog logFile, WriteModel writeModel)
        {
            _logFile = logFile;
            WriteModel = writeModel;
        }

        #region Node
     

        public string When(AddNodeIntoFiber cmd)
        {
//            if (!cmd.IsAdjustmentPoint && WriteModel.IsFiberContainedInAnyTraceWithBase(cmd.FiberId))
//                return Resources.SID_It_s_impossible_to_change_trace_with_base_reflectogram;

            return WriteModel.Add(_mapper.Map<NodeIntoFiberAdded>(cmd));
        }

        public string When(UpdateNode cmd)
        {
            return WriteModel.Add(_mapper.Map<NodeUpdated>(cmd));
        }


        public string When(MoveNode cmd)
        {
            return WriteModel.Add(_mapper.Map<NodeMoved>(cmd));
        }

        public string When(RemoveNode cmd)
        {
            if (WriteModel.IsNodeLastForAnyTrace(cmd.Id))
                return Resources.SID_It_s_prohibited_to_remove_last_node_from_trace;
            if (WriteModel.IsNodeContainedInAnyTraceWithBase(cmd.Id) && cmd.Type != EquipmentType.AdjustmentPoint)
                return Resources.SID_It_s_impossible_to_change_trace_with_base_reflectogram;

            return WriteModel.Add(_mapper.Map<NodeRemoved>(cmd));
        }
        #endregion

        #region Fiber
        public string When(AddFiber cmd)
        {
            if (WriteModel.HasFiberBetween(cmd.Node1, cmd.Node2))
                return Resources.SID_Section_already_exists;

            return WriteModel.Add(_mapper.Map<FiberAdded>(cmd));
        }

        public string When(AddFiberWithNodes cmd)
        {
            if (WriteModel.HasFiberBetween(cmd.Node1, cmd.Node2))
                return Resources.SID_Section_already_exists;


            foreach (var cmdAddEquipmentAtGpsLocation in cmd.AddEquipments)
            {
                var result = WriteModel.Add(_mapper.Map<EquipmentAtGpsLocationAdded>(cmdAddEquipmentAtGpsLocation));
                if (result != null)
                    return result;
            }

            foreach (var cmdAddFiber in cmd.AddFibers)
            {
                var result = WriteModel.Add(_mapper.Map<FiberAdded>(cmdAddFiber));
                if (result != null)
                    return result;
            }

            return null;
        }

        public string When(UpdateFiber cmd)
        {
            return WriteModel.Add(_mapper.Map<FiberUpdated>(cmd));
        }
        public string When(RemoveFiber cmd)
        {
            if (WriteModel.IsFiberContainedInAnyTraceWithBase(cmd.Id))
                return Resources.SID_It_s_impossible_to_change_trace_with_base_reflectogram;
            return WriteModel.Add(_mapper.Map<FiberRemoved>(cmd));
        }
        #endregion

        #region Equipment
        public string When(AddEquipmentIntoNode cmd)
        {
            foreach (var traceId in cmd.TracesForInsertion)
            {
                var trace = WriteModel.GetTrace(traceId);
                if (trace == null)
                {
                    var message = $@"AddEquipmentIntoNode: Trace {traceId.First6()} not found";
                    _logFile.AppendLine(message);
                    return message;
                }
                if (trace.HasAnyBaseRef)
                    return Resources.SID_Base_ref_is_set_for_trace;
            }
            var result = WriteModel.Add(_mapper.Map<EquipmentIntoNodeAdded>(cmd));
            if (result != null)
                return result;

            foreach (var traceId in cmd.TracesForInsertion)
            {
                var trace = WriteModel.GetTrace(traceId);
                if (trace == null)
                {
                    var message = $@"AddEquipmentIntoNode: Trace {traceId.First6()} not found";
                    _logFile.AppendLine(message);
                    return message;
                }
                var idx = trace.Nodes.IndexOf(cmd.NodeId);
                trace.Equipments[idx] = cmd.Id;
            }
            return null;
        }

        public string When(AddEquipmentAtGpsLocation cmd)
        {
            return WriteModel.Add(_mapper.Map<EquipmentAtGpsLocationAdded>(cmd));
        }

        public string When(UpdateEquipment cmd)
        {
            return WriteModel.Add(_mapper.Map<EquipmentUpdated>(cmd));
        }
        public string When(RemoveEquipment cmd)
        {
            return WriteModel.Add(_mapper.Map<EquipmentRemoved>(cmd));
        }
        #endregion

        #region Rtu
        public string When(AddRtuAtGpsLocation cmd)
        {
            return WriteModel.Add(_mapper.Map<RtuAtGpsLocationAdded>(cmd));
        }

        public string When(UpdateRtu cmd)
        {
            return WriteModel.Add(_mapper.Map<RtuUpdated>(cmd));
        }

        public string When(RemoveRtu cmd)
        {
            return WriteModel.Add(_mapper.Map<RtuRemoved>(cmd));
        }

        public string When(AttachOtau cmd)
        {
            return WriteModel.Add(_mapper.Map<OtauAttached>(cmd));
        }

        public string When(DetachOtau cmd)
        {
            return WriteModel.Add(_mapper.Map<OtauDetached>(cmd));
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

            return WriteModel.Add(_mapper.Map<TraceAdded>(cmd));
        }

        public string When(UpdateTrace cmd)
        {
            return WriteModel.Add(_mapper.Map<TraceUpdated>(cmd));
        }

        public string When(CleanTrace cmd)
        {
            return WriteModel.Add(_mapper.Map<TraceCleaned>(cmd));
        }

        public string When(RemoveTrace cmd)
        {
            return WriteModel.Add(_mapper.Map<TraceRemoved>(cmd));
        }

        public string When(AttachTrace cmd)
        {
            return WriteModel.Add(_mapper.Map<TraceAttached>(cmd));
        }

        public string When(DetachTrace cmd)
        {
            return WriteModel.Add(_mapper.Map<TraceDetached>(cmd));
        }
        #endregion

        #region JustEchosOfCmdsSentToRtu
        public string When(AssignBaseRef cmd)
        {
            return WriteModel.Add(_mapper.Map<BaseRefAssigned>(cmd));
        }
        public string When(ChangeMonitoringSettings cmd)
        {
            return WriteModel.Add(_mapper.Map<MonitoringSettingsChanged>(cmd));
        }

        public string When(InitializeRtu cmd)
        {
            return WriteModel.Add(_mapper.Map<RtuInitialized>(cmd));
        }

        public string When(StartMonitoring cmd)
        {
            return WriteModel.Add(_mapper.Map<MonitoringStarted>(cmd));
        }

        public string When(StopMonitoring cmd)
        {
            return WriteModel.Add(_mapper.Map<MonitoringStopped>(cmd));
        }
        #endregion
    }
}
