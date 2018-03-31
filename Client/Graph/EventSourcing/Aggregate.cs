using System;
using System.Collections.Generic;
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


        #region User
        public string When(AddUser cmd)
        {
            return WriteModel.Add(_mapper.Map<UserAdded>(cmd));
        }

        public string When(UpdateUser cmd)
        {
            return WriteModel.Add(_mapper.Map<UserUpdated>(cmd));
        }

        public string When(RemoveUser cmd)
        {
            return WriteModel.Add(_mapper.Map<UserRemoved>(cmd));
        }
        #endregion

        #region Zone
        public string When(AddZone cmd)
        {
            return WriteModel.Add(_mapper.Map<ZoneAdded>(cmd));
        }

        public string When(UpdateZone cmd)
        {
            return WriteModel.Add(_mapper.Map<ZoneUpdated>(cmd));
        }

        public string When(RemoveZone cmd)
        {
            // Checks?
            return WriteModel.Add(_mapper.Map<ZoneRemoved>(cmd));
        }
        #endregion

        #region Node
        public string When(AddNodeIntoFiber cmd)
        {
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
            if (WriteModel.IsNodeLastForAnyTrace(cmd.NodeId))
                return Resources.SID_It_s_prohibited_to_remove_last_node_from_trace;
            if (WriteModel.IsNodeContainedInAnyTraceWithBase(cmd.NodeId) && cmd.Type != EquipmentType.AdjustmentPoint)
                return Resources.SID_It_s_impossible_to_change_trace_with_base_reflectogram;

            return WriteModel.Add(_mapper.Map<NodeRemoved>(cmd));
        }
        #endregion

        #region Fiber
        public string When(AddFiber cmd)
        {
            Guid a = cmd.NodeId1;
            Guid b = cmd.NodeId2;
            if (WriteModel.Fibers.Any(f =>
                f.NodeId1 == a && f.NodeId2 == b ||
                f.NodeId1 == b && f.NodeId2 == a))
                return Resources.SID_Section_already_exists;

            return WriteModel.Add(_mapper.Map<FiberAdded>(cmd));
        }

        public string When(AddFiberWithNodes cmd)
        {
            Guid a = cmd.Node1;
            Guid b = cmd.Node2;
            if (WriteModel.Fibers.Any(f =>
                f.NodeId1 == a && f.NodeId2 == b ||
                f.NodeId1 == b && f.NodeId2 == a))
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
            if (WriteModel.IsFiberContainedInAnyTraceWithBase(cmd.FiberId))
                return Resources.SID_It_s_impossible_to_change_trace_with_base_reflectogram;
            return WriteModel.Add(_mapper.Map<FiberRemoved>(cmd));
        }
        #endregion

        #region Equipment
        public string When(AddEquipmentIntoNode cmd)
        {
            foreach (var traceId in cmd.TracesForInsertion)
            {
                var trace = WriteModel.Traces.FirstOrDefault(t => t.TraceId == traceId);
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
                var trace = WriteModel.Traces.FirstOrDefault(t => t.TraceId == traceId);
                if (trace == null)
                {
                    var message = $@"AddEquipmentIntoNode: Trace {traceId.First6()} not found";
                    _logFile.AppendLine(message);
                    return message;
                }
                var idx = trace.NodeIds.IndexOf(cmd.NodeId);
                trace.EquipmentIds[idx] = cmd.EquipmentId;
            }
            return null;
        }

        public string When(AddEquipmentAtGpsLocation cmd)
        {
            return WriteModel.Add(_mapper.Map<EquipmentAtGpsLocationAdded>(cmd));
        }

        public string When(AddEquipmentAtGpsLocationWithNodeTitle cmd)
        {
            return WriteModel.Add(_mapper.Map<EquipmentAtGpsLocationWithNodeTitleAdded>(cmd));
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
            var evnt = _mapper.Map<RtuRemoved>(cmd);
            evnt.FibersFromCleanedTraces = new Dictionary<Guid, Guid>();
            foreach (var trace in WriteModel.Traces.Where(t=>t.RtuId == cmd.RtuId))
            {
                foreach (var fiberId in WriteModel.GetFibersByNodes(trace.NodeIds))
                {
                    evnt.FibersFromCleanedTraces.Add(fiberId, trace.TraceId);
                }
            }
            return WriteModel.Add(evnt);
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
            var rtu = WriteModel.Rtus.FirstOrDefault(r => r.Id == cmd.RtuId);
            if (rtu == null)
                return Resources.SID_RTU_is_not_found;
            if (cmd.EquipmentIds[0] != cmd.RtuId)
                return Resources.SID_Trace_should_start_from_RTU;
            if (cmd.NodeIds.Count != cmd.EquipmentIds.Count)
                return Resources.SID_Equipments_count_in_trace_should_match_nodes_count;
            if (cmd.EquipmentIds.Last() == Guid.Empty)
                return Resources.SID_Last_node_of_trace_must_contain_some_equipment;

            return WriteModel.Add(_mapper.Map<TraceAdded>(cmd));
        }

        public string When(UpdateTrace cmd)
        {
            return WriteModel.Add(_mapper.Map<TraceUpdated>(cmd));
        }

        public string When(CleanTrace cmd)
        {
            var traceCleaned = _mapper.Map<TraceCleaned>(cmd);
            traceCleaned.NodeIds = WriteModel.Traces.First(t => t.TraceId == cmd.TraceId).NodeIds;
            traceCleaned.FiberIds = WriteModel.GetFibersByNodes(traceCleaned.NodeIds).ToList();
            return WriteModel.Add(traceCleaned);
        }

        public string When(RemoveTrace cmd)
        {
            var traceRemoved = _mapper.Map<TraceRemoved>(cmd);
            traceRemoved.NodeIds = WriteModel.Traces.First(t => t.TraceId == cmd.TraceId).NodeIds;
            traceRemoved.FiberIds = WriteModel.GetFibersByNodes(traceRemoved.NodeIds).ToList();
            return WriteModel.Add(traceRemoved);
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

        public string When(AddMeasurement cmd)
        {
            if (WriteModel.Traces.All(t => t.TraceId != cmd.TraceId))
                return $@"Unknown trace {cmd.TraceId.First6()}";
            return WriteModel.Add(_mapper.Map<MeasurementAdded>(cmd));
        }
        public string When(UpdateMeasurement cmd)
        {
            return WriteModel.Add(_mapper.Map<MeasurementUpdated>(cmd));
        }

        public string When(AddNetworkEvent cmd)
        {
            var networkEventAdded = _mapper.Map<NetworkEventAdded>(cmd);
            var lastEventOrdial = WriteModel.NetworkEvents.Any() ?  WriteModel.NetworkEvents.Max(n => n.Ordinal) : 1;
            networkEventAdded.Ordinal = lastEventOrdial + 1;
            var rtu = WriteModel.Rtus.First(r => r.Id == networkEventAdded.RtuId);
            networkEventAdded.RtuPartStateChanges = IsStateWorseOrBetterThanBefore(rtu, networkEventAdded);
            return WriteModel.Add(networkEventAdded);
        }

        public string When(AddBopNetworkEvent cmd)
        {
            return WriteModel.Add(_mapper.Map<BopNetworkEventAdded>(cmd));
        }


        private RtuPartStateChanges IsStateWorseOrBetterThanBefore(Rtu rtu, NetworkEventAdded networkEvent)
        {
            List<WorseOrBetter> parts = new List<WorseOrBetter> {
                rtu.MainChannelState.BecomeBetterOrWorse(networkEvent.MainChannelState),
                rtu.ReserveChannelState.BecomeBetterOrWorse(networkEvent.ReserveChannelState),
            };

            if (parts.Contains(WorseOrBetter.Worse) && parts.Contains(WorseOrBetter.Better))
                return RtuPartStateChanges.DifferentPartsHaveDifferentChanges;
            if (parts.Contains(WorseOrBetter.Worse))
                return RtuPartStateChanges.OnlyWorse;
            if (parts.Contains(WorseOrBetter.Better))
                return RtuPartStateChanges.OnlyBetter;
            return RtuPartStateChanges.NoChanges;
        }


        public string When(ChangeResponsibilities cmd)
        {
            return WriteModel.Add(_mapper.Map<ResponsibilitiesChanged>(cmd));
        }

        #endregion
    }
}
