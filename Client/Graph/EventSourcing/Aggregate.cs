using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph.Algorithms;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Graph
{
    public class Aggregate
    {
        private readonly IMyLog _logFile;
        private readonly IModel _writeModel;

        public EventsQueue EventsQueue { get; }

        private static readonly IMapper Mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingCmdToEventProfile>()).CreateMapper();

        public Aggregate(IMyLog logFile, EventsQueue eventsQueue, IModel writeModel)
        {
            _logFile = logFile;
            _writeModel = writeModel;
            EventsQueue = eventsQueue;
        }


        #region User
        public string When(AddUser cmd)
        {
            return EventsQueue.Add(Mapper.Map<UserAdded>(cmd));
        }

        public string When(UpdateUser cmd)
        {
            return EventsQueue.Add(Mapper.Map<UserUpdated>(cmd));
        }

        public string When(RemoveUser cmd)
        {
            return EventsQueue.Add(Mapper.Map<UserRemoved>(cmd));
        }
        #endregion

        #region Zone
        public string When(AddZone cmd)
        {
            return EventsQueue.Add(Mapper.Map<ZoneAdded>(cmd));
        }

        public string When(UpdateZone cmd)
        {
            return EventsQueue.Add(Mapper.Map<ZoneUpdated>(cmd));
        }

        public string When(RemoveZone cmd)
        {
            // Checks?
            return EventsQueue.Add(Mapper.Map<ZoneRemoved>(cmd));
        }
        #endregion

        #region Node
        public string When(AddNodeIntoFiber cmd)
        {
            return EventsQueue.Add(Mapper.Map<NodeIntoFiberAdded>(cmd));
        }

        public string When(UpdateNode cmd)
        {
            return EventsQueue.Add(Mapper.Map<NodeUpdated>(cmd));
        }


        public string When(MoveNode cmd)
        {
            return EventsQueue.Add(Mapper.Map<NodeMoved>(cmd));
        }

        public string When(RemoveNode cmd)
        {
            if (_writeModel.Traces.Any(t => t.NodeIds.Last() == cmd.NodeId))
                return Resources.SID_It_s_prohibited_to_remove_last_node_from_trace;
            if (_writeModel.Traces.Any(t => t.HasAnyBaseRef && t.NodeIds.Contains(cmd.NodeId) && cmd.Type != EquipmentType.AdjustmentPoint))
                return Resources.SID_It_s_impossible_to_change_trace_with_base_reflectogram;

            return EventsQueue.Add(Mapper.Map<NodeRemoved>(cmd));
        }
        #endregion

        #region Fiber
        public string When(AddFiber cmd)
        {
            Guid a = cmd.NodeId1;
            Guid b = cmd.NodeId2;
            if (_writeModel.Fibers.Any(f =>
                f.NodeId1 == a && f.NodeId2 == b ||
                f.NodeId1 == b && f.NodeId2 == a))
                return Resources.SID_Section_already_exists;

            return EventsQueue.Add(Mapper.Map<FiberAdded>(cmd));
        }

        public string When(AddFiberWithNodes cmd)
        {
            Guid a = cmd.Node1;
            Guid b = cmd.Node2;
            if (_writeModel.Fibers.Any(f =>
                f.NodeId1 == a && f.NodeId2 == b ||
                f.NodeId1 == b && f.NodeId2 == a))
                return Resources.SID_Section_already_exists;


            foreach (var cmdAddEquipmentAtGpsLocation in cmd.AddEquipments)
            {
                var result = EventsQueue.Add(Mapper.Map<EquipmentAtGpsLocationAdded>(cmdAddEquipmentAtGpsLocation));
                if (result != null)
                    return result;
            }

            foreach (var cmdAddFiber in cmd.AddFibers)
            {
                var result = EventsQueue.Add(Mapper.Map<FiberAdded>(cmdAddFiber));
                if (result != null)
                    return result;
            }

            return null;
        }

        public string When(UpdateFiber cmd)
        {
            return EventsQueue.Add(Mapper.Map<FiberUpdated>(cmd));
        }
        public string When(RemoveFiber cmd)
        {
            if (IsFiberContainedInAnyTraceWithBase(cmd.FiberId))
                return Resources.SID_It_s_impossible_to_change_trace_with_base_reflectogram;
            return EventsQueue.Add(Mapper.Map<FiberRemoved>(cmd));
        }

        private bool IsFiberContainedInAnyTraceWithBase(Guid fiberId)
        {
            var tracesWithBase = _writeModel.Traces.Where(t => t.HasAnyBaseRef);
            var fiber = _writeModel.Fibers.FirstOrDefault(f => f.FiberId == fiberId);
            if (fiber == null)
            {
                _logFile.AppendLine($@"IsFiberContainedInAnyTraceWithBase: Fiber {fiberId.First6()} not found");
            }
            return tracesWithBase.Any(trace => Topo.GetFiberIndexInTrace(trace, fiber) != -1);
        }
        #endregion

        #region Equipment
        public string When(AddEquipmentIntoNode cmd)
        {
            foreach (var traceId in cmd.TracesForInsertion)
            {
                var trace = _writeModel.Traces.FirstOrDefault(t => t.TraceId == traceId);
                if (trace == null)
                {
                    var message = $@"AddEquipmentIntoNode: Trace {traceId.First6()} not found";
                    _logFile.AppendLine(message);
                    return message;
                }
                if (trace.HasAnyBaseRef)
                    return Resources.SID_Base_ref_is_set_for_trace;
            }
            var result = EventsQueue.Add(Mapper.Map<EquipmentIntoNodeAdded>(cmd));
            if (result != null)
                return result;

            foreach (var traceId in cmd.TracesForInsertion)
            {
                var trace = _writeModel.Traces.FirstOrDefault(t => t.TraceId == traceId);
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
            return EventsQueue.Add(Mapper.Map<EquipmentAtGpsLocationAdded>(cmd));
        }

        public string When(AddEquipmentAtGpsLocationWithNodeTitle cmd)
        {
            return EventsQueue.Add(Mapper.Map<EquipmentAtGpsLocationWithNodeTitleAdded>(cmd));
        }

        public string When(UpdateEquipment cmd)
        {
            return EventsQueue.Add(Mapper.Map<EquipmentUpdated>(cmd));
        }
        public string When(RemoveEquipment cmd)
        {
            return EventsQueue.Add(Mapper.Map<EquipmentRemoved>(cmd));
        }
        #endregion

        #region Rtu
        public string When(AddRtuAtGpsLocation cmd)
        {
            return EventsQueue.Add(Mapper.Map<RtuAtGpsLocationAdded>(cmd));
        }

        public string When(UpdateRtu cmd)
        {
            return EventsQueue.Add(Mapper.Map<RtuUpdated>(cmd));
        }

        public string When(RemoveRtu cmd)
        {
            var evnt = Mapper.Map<RtuRemoved>(cmd);
            evnt.FibersFromCleanedTraces = new Dictionary<Guid, Guid>();
            foreach (var trace in _writeModel.Traces.Where(t=>t.RtuId == cmd.RtuId))
            {
                foreach (var fiberId in _writeModel.GetFibersByNodes(trace.NodeIds))
                {
                    evnt.FibersFromCleanedTraces.Add(fiberId, trace.TraceId);
                }
            }
            return EventsQueue.Add(evnt);
        }

        public string When(AttachOtau cmd)
        {
            return EventsQueue.Add(Mapper.Map<OtauAttached>(cmd));
        }

        public string When(DetachOtau cmd)
        {
            return EventsQueue.Add(Mapper.Map<OtauDetached>(cmd));
        }
        #endregion

        #region Trace
        public string When(AddTrace cmd)
        {
            var rtu = _writeModel.Rtus.FirstOrDefault(r => r.Id == cmd.RtuId);
            if (rtu == null)
                return Resources.SID_RTU_is_not_found;
            if (cmd.EquipmentIds[0] != cmd.RtuId)
                return Resources.SID_Trace_should_start_from_RTU;
            if (cmd.NodeIds.Count != cmd.EquipmentIds.Count)
                return Resources.SID_Equipments_count_in_trace_should_match_nodes_count;
            if (cmd.EquipmentIds.Last() == Guid.Empty)
                return Resources.SID_Last_node_of_trace_must_contain_some_equipment;

            return EventsQueue.Add(Mapper.Map<TraceAdded>(cmd));
        }

        public string When(UpdateTrace cmd)
        {
            return EventsQueue.Add(Mapper.Map<TraceUpdated>(cmd));
        }

        public string When(CleanTrace cmd)
        {
            var traceCleaned = Mapper.Map<TraceCleaned>(cmd);
            traceCleaned.NodeIds = _writeModel.Traces.First(t => t.TraceId == cmd.TraceId).NodeIds;
            traceCleaned.FiberIds = _writeModel.GetFibersByNodes(traceCleaned.NodeIds).ToList();
            return EventsQueue.Add(traceCleaned);
        }

        public string When(RemoveTrace cmd)
        {
            var traceRemoved = Mapper.Map<TraceRemoved>(cmd);
            traceRemoved.NodeIds = _writeModel.Traces.First(t => t.TraceId == cmd.TraceId).NodeIds;
            traceRemoved.FiberIds = _writeModel.GetFibersByNodes(traceRemoved.NodeIds).ToList();
            return EventsQueue.Add(traceRemoved);
        }

        public string When(AttachTrace cmd)
        {
            return EventsQueue.Add(Mapper.Map<TraceAttached>(cmd));
        }

        public string When(DetachTrace cmd)
        {
            return EventsQueue.Add(Mapper.Map<TraceDetached>(cmd));
        }
        #endregion

        #region JustEchosOfCmdsSentToRtu
        public string When(AssignBaseRef cmd)
        {
            return EventsQueue.Add(Mapper.Map<BaseRefAssigned>(cmd));
        }
        public string When(ChangeMonitoringSettings cmd)
        {
            return EventsQueue.Add(Mapper.Map<MonitoringSettingsChanged>(cmd));
        }

        public string When(InitializeRtu cmd)
        {
            return EventsQueue.Add(Mapper.Map<RtuInitialized>(cmd));
        }

        public string When(StartMonitoring cmd)
        {
            return EventsQueue.Add(Mapper.Map<MonitoringStarted>(cmd));
        }

        public string When(StopMonitoring cmd)
        {
            return EventsQueue.Add(Mapper.Map<MonitoringStopped>(cmd));
        }

        public string When(AddMeasurement cmd)
        {
            if (_writeModel.Traces.All(t => t.TraceId != cmd.TraceId))
                return $@"Unknown trace {cmd.TraceId.First6()}";
            return EventsQueue.Add(Mapper.Map<MeasurementAdded>(cmd));
        }
        public string When(UpdateMeasurement cmd)
        {
            return EventsQueue.Add(Mapper.Map<MeasurementUpdated>(cmd));
        }

        public string When(AddNetworkEvent cmd)
        {
            var networkEventAdded = Mapper.Map<NetworkEventAdded>(cmd);
            var lastEventOrdial = _writeModel.NetworkEvents.Any() ? _writeModel.NetworkEvents.Max(n => n.Ordinal) : 1;
            networkEventAdded.Ordinal = lastEventOrdial + 1;
            var rtu = _writeModel.Rtus.First(r => r.Id == networkEventAdded.RtuId);
            networkEventAdded.RtuPartStateChanges = IsStateWorseOrBetterThanBefore(rtu, networkEventAdded);
            return EventsQueue.Add(networkEventAdded);
        }

        public string When(AddBopNetworkEvent cmd)
        {
            return EventsQueue.Add(Mapper.Map<BopNetworkEventAdded>(cmd));
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
            return EventsQueue.Add(Mapper.Map<ResponsibilitiesChanged>(cmd));
        }

        #endregion
    }
}
