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

        public Aggregate(IMyLog logFile,  WriteModel writeModel)
        {
            _logFile = logFile;
            WriteModel = writeModel;
        }

        #region Node
        public string When(AddNode cmd)
        {
            WriteModel.Add(_mapper.Map<NodeAdded>(cmd));
            return null;
        }

        public string When(AddNodeIntoFiber cmd)
        {
            if (WriteModel.IsFiberContainedInAnyTraceWithBase(cmd.FiberId))
                return Resources.SID_It_s_impossible_to_change_trace_with_base_reflectogram;

            WriteModel.Add(_mapper.Map<NodeIntoFiberAdded>(cmd));
            return null;
        }

        public string When(UpdateNode cmd)
        {
            WriteModel.Add(_mapper.Map<NodeUpdated>(cmd));
            return null;
        }


        public void When(MoveNode cmd)
        {
            WriteModel.Add(_mapper.Map<NodeMoved>(cmd));
        }

        public string When(RemoveNode cmd)
        {
            if (WriteModel.IsNodeLastForAnyTrace(cmd.Id))
                return Resources.SID_It_s_prohibited_to_remove_last_node_from_trace;
            if (WriteModel.IsNodeContainedInAnyTraceWithBase(cmd.Id))
                return Resources.SID_It_s_impossible_to_change_trace_with_base_reflectogram;

            WriteModel.Add(_mapper.Map<NodeRemoved>(cmd));
            return null;
        }
        #endregion

        #region Fiber

        public string When(AddFiber cmd)
        {
            if (WriteModel.HasFiberBetween(cmd.Node1, cmd.Node2))
                return Resources.SID_Section_already_exists;

            WriteModel.Add(_mapper.Map<FiberAdded>(cmd));
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

            return null;
        }

        public string When(UpdateFiber cmd)
        {
            WriteModel.Add(_mapper.Map<FiberUpdated>(cmd));
            return null;
        }
        public string When(RemoveFiber cmd)
        {
            if (WriteModel.IsFiberContainedInAnyTraceWithBase(cmd.Id))
                return Resources.SID_It_s_impossible_to_change_trace_with_base_reflectogram;
            WriteModel.Add(_mapper.Map<FiberRemoved>(cmd));
            return null;
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
                    _logFile.AppendLine($@"AddEquipmentIntoNode: Trace {traceId.First6()} not found");
                    break;
                }
                if (trace.HasBase)
                    return Resources.SID_Base_ref_is_set_for_trace;
            }
            WriteModel.Add(_mapper.Map<EquipmentIntoNodeAdded>(cmd));

            foreach (var traceId in cmd.TracesForInsertion)
            {
                var trace = WriteModel.GetTrace(traceId);
                if (trace == null)
                {
                    _logFile.AppendLine($@"AddEquipmentIntoNode: Trace {traceId.First6()} not found");
                    break;
                }
                var idx = trace.Nodes.IndexOf(cmd.NodeId);
                trace.Equipments[idx] = cmd.Id;
            }
            return null;
        }

        public string When(AddEquipmentAtGpsLocation cmd)
        {
            WriteModel.Add(_mapper.Map<EquipmentAtGpsLocationAdded>(cmd));
            return null;
        }

        public string When(UpdateEquipment cmd)
        {
            //            if (WriteModel.GetEquipment(cmd.RtuId) == null)
            //                return "Somebody removed this equipment while you updated it";
            WriteModel.Add(_mapper.Map<EquipmentUpdated>(cmd));
            return null;
        }
        public string When(RemoveEquipment cmd)
        {
            WriteModel.Add(_mapper.Map<EquipmentRemoved>(cmd));
            return null;
        }
        #endregion

        #region Rtu
        public string When(AddRtuAtGpsLocation cmd)
        {
            WriteModel.Add(_mapper.Map<RtuAtGpsLocationAdded>(cmd));
            return null;
        }

        public string When(InitializeRtu cmd)
        {
            var evnt = _mapper.Map<RtuInitialized>(cmd);
            WriteModel.Add(evnt);
            return null;
        }

        public string When(ChangeMonitoringSettings cmd)
        {
            var evnt = _mapper.Map<MonitoringSettingsChanged>(cmd);
            WriteModel.Add(evnt);
            return null;
        }

        public string When(UpdateRtu cmd)
        {
            WriteModel.Add(_mapper.Map<RtuUpdated>(cmd));
            return null;
        }

        public string When(RemoveRtu cmd)
        {
            WriteModel.Add(_mapper.Map<RtuRemoved>(cmd));
            return null;
        }
        public string When(AttachOtau cmd)
        {
            WriteModel.Add(_mapper.Map<OtauAttached>(cmd));
            return null;
        }
        public string When(DetachOtau cmd)
        {
            WriteModel.Add(_mapper.Map<OtauDetached>(cmd));
            return null;
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

            WriteModel.Add(_mapper.Map<TraceAdded>(cmd));
            return null;
            //_traces.Add(_mapper2.Map<Trace>(cmd));
        }

        public string When(UpdateTrace cmd)
        {
            WriteModel.Add(_mapper.Map<TraceUpdated>(cmd));
            return null;
        }

        public string When(CleanTrace cmd)
        {
            WriteModel.Add(_mapper.Map<TraceCleaned>(cmd));
            return null;
        }

        public string When(RemoveTrace cmd)
        {
            WriteModel.Add(_mapper.Map<TraceRemoved>(cmd));
            return null;
        }

        public string When(AttachTrace cmd)
        {
            WriteModel.Add(_mapper.Map<TraceAttached>(cmd));
            return null;
        }

        public string When(DetachTrace cmd)
        {
            WriteModel.Add(_mapper.Map<TraceDetached>(cmd));
            return null;
        }

        public string When(AssignBaseRef cmd)
        {
            WriteModel.Add(_mapper.Map<BaseRefAssigned>(cmd));
            var trace = WriteModel.GetTrace(cmd.TraceId);
            if (trace == null)
            {
                var message = $@"AssignBaseRef: Trace {cmd.TraceId.First6()} not found";
                _logFile.AppendLine(message);
                return message;
            }

            var preciseBaseRef = cmd.BaseRefs.FirstOrDefault(b => b.BaseRefType == BaseRefType.Precise);
            if (preciseBaseRef != null)
                trace.PreciseId = preciseBaseRef.Id;
            var fastBaseRef = cmd.BaseRefs.FirstOrDefault(b => b.BaseRefType == BaseRefType.Fast);
            if (fastBaseRef != null)
                trace.FastId = fastBaseRef.Id;
            var additionalBaseRef = cmd.BaseRefs.FirstOrDefault(b => b.BaseRefType == BaseRefType.Additional);
            if (additionalBaseRef != null)
                trace.AdditionalId = additionalBaseRef.Id;
            return null;
        }
        #endregion
    }
}
