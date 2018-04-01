﻿using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GMap.NET;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph.Algorithms;
using Iit.Fibertest.UtilsLib;
using PrivateReflectionUsingDynamic;

namespace Iit.Fibertest.Graph
{
    public class WriteModel : IModel
    {
        public IMyLog LogFile { get; }

        private static readonly IMapper Mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingEventToDomainModelProfile>()).CreateMapper();

        public List<Node> Nodes { get; } = new List<Node>();
        public List<Fiber> Fibers { get; } = new List<Fiber>();
        public List<Equipment> Equipments { get; } = new List<Equipment>();
        public List<Trace> Traces { get; } = new List<Trace>();
        public List<Rtu> Rtus { get; } = new List<Rtu>();
        public List<Otau> Otaus { get; } = new List<Otau>();
        public List<User> Users { get; } = new List<User>();
        public List<Zone> Zones { get; } = new List<Zone>();
        public List<Measurement> Measurements { get; } = new List<Measurement>();
        public List<NetworkEvent> NetworkEvents { get; } = new List<NetworkEvent>();
        public List<BopNetworkEvent> BopNetworkEvents { get; } = new List<BopNetworkEvent>();
        public List<BaseRef> BaseRefs { get; } = new List<BaseRef>();

        public WriteModel(IMyLog logFile)
        {
            LogFile = logFile;
        }

        public void Init(IEnumerable<object> events)
        {
            foreach (var dbEvent in events)
            {
                this.AsDynamic().Apply(dbEvent);
            }
        }

        public string Add(object evnt)
        {
            var result = (string)this.AsDynamic().Apply(evnt);
      
            return result;
        }

      

        #region User
        public string Apply(UserAdded e)
        {
            Users.Add(Mapper.Map<User>(e));
            return null;
        }

        public string Apply(UserUpdated source)
        {
            var destination = Users.First(f => f.UserId == source.UserId);
            Mapper.Map(source, destination);
            return null;
        }

        public string Apply(UserRemoved e)
        {
            Users.Remove(Users.First(f => f.UserId == e.UserId));
            return null;
        }
        #endregion

        #region Zone
        public string Apply(ZoneAdded e)
        {
            Zones.Add(Mapper.Map<Zone>(e));
            return null;
        }

        public string Apply(ZoneUpdated source)
        {
            var destination = Zones.First(f => f.ZoneId == source.ZoneId);
            Mapper.Map(source, destination);
            return null;
        }

        public string Apply(ZoneRemoved e)
        {
            Zones.Remove(Zones.First(f => f.ZoneId == e.ZoneId));
            return null;
        }
        #endregion

        #region Node

        public string Apply(NodeIntoFiberAdded e)
        {
            Nodes.Add(new Node() { NodeId = e.Id, Position = e.Position });
            Equipments.Add(new Equipment() { EquipmentId = e.EquipmentId, Type = e.InjectionType, NodeId = e.Id });
            AddTwoFibersToNewNode(e);
            FixTracesWhichContainedOldFiber(e);
            var fiber = Fibers.FirstOrDefault(f => f.FiberId == e.FiberId);
            if (fiber != null)
            {
                Fibers.Remove(fiber);
                return null;
            }

            var message = $@"NodeIntoFiberAdded: Fiber {e.FiberId.First6()} not found";
            LogFile.AppendLine(message);
            return message;
        }

        private void FixTracesWhichContainedOldFiber(NodeIntoFiberAdded e)
        {
            foreach (var trace in Traces)
            {
                int idx;
                while ((idx = Topo.GetFiberIndexInTrace(trace, Fibers.Single(f => f.FiberId == e.FiberId))) != -1)
                {
                    trace.NodeIds.Insert(idx + 1, e.Id); // GPS location добавляется во все трассы
                    trace.EquipmentIds.Insert(idx + 1, e.EquipmentId); // GPS location добавляется во все трассы
                }
            }
        }
        private void AddTwoFibersToNewNode(NodeIntoFiberAdded e)
        {
            var fiber = Fibers.FirstOrDefault(f => f.FiberId == e.FiberId);
            if (fiber == null)
            {
                LogFile.AppendLine($@"AddTwoFibersToNewNode: Fiber {e.FiberId.First6()} not found");
                return;
            }
            Guid nodeId1 = fiber.NodeId1;
            Guid nodeId2 = fiber.NodeId2;

            Fibers.Add(new Fiber() { FiberId = e.NewFiberId1, NodeId1 = nodeId1, NodeId2 = e.Id });
            Fibers.Add(new Fiber() { FiberId = e.NewFiberId2, NodeId1 = e.Id, NodeId2 = nodeId2 });
        }

        public string Apply(NodeUpdated source)
        {
            var node = Nodes.FirstOrDefault(x => x.NodeId == source.NodeId);
            if (node != null)
            {
                Mapper.Map(source, node);
                return null;
            }

            var message = $@"NodeUpdated: Node {source.NodeId.First6()} not found";
            LogFile.AppendLine(message);
            return message;
        }

        public string Apply(NodeMoved e)
        {
            var node = Nodes.FirstOrDefault(x => x.NodeId == e.NodeId);
            if (node != null)
            {
                node.Position = new PointLatLng(e.Latitude, e.Longitude);
                return null;
            }

            var message = $@"NodeMoved: Node {e.NodeId.First6()} not found";
            LogFile.AppendLine(message);
            return message;
        }

        public string Apply(NodeRemoved e)
        {
            foreach (var trace in Traces.Where(t => t.NodeIds.Contains(e.NodeId)))
            {
                if (e.TraceWithNewFiberForDetourRemovedNode == null ||
                    !e.TraceWithNewFiberForDetourRemovedNode.ContainsKey(trace.TraceId))
                {
                    var message = $@"NodeRemoved: No fiber prepared to detour trace {trace.TraceId}";
                    LogFile.AppendLine(message);
                    return message;
                }
                else
                    ExcludeNodeFromTrace(trace, e.TraceWithNewFiberForDetourRemovedNode[trace.TraceId], e.NodeId);
            }

            if (e.FiberIdToDetourAdjustmentPoint != Guid.Empty)
                return ExcludeAdjustmentPoint(e.NodeId, e.FiberIdToDetourAdjustmentPoint);

            if (e.TraceWithNewFiberForDetourRemovedNode.Count == 0 &&
                Fibers.Count(f => f.NodeId1 == e.NodeId || f.NodeId2 == e.NodeId) == 1)
                return RemoveNodeOnEdgeWhereNoTraces(e.NodeId);
            return RemoveNodeWithAllHis(e.NodeId);
        }

        private void ExcludeNodeFromTrace(Trace trace, Guid fiberId, Guid nodeId)
        {
            var idxInTrace = trace.NodeIds.IndexOf(nodeId);
            CreateDetourIfAbsent(trace, fiberId, idxInTrace);

            trace.EquipmentIds.RemoveAt(idxInTrace);
            trace.NodeIds.RemoveAt(idxInTrace);
        }

        private string ExcludeAdjustmentPoint(Guid nodeId, Guid detourFiberId)
        {
            var leftFiber = Fibers.FirstOrDefault(f => f.NodeId2 == nodeId);
            if (leftFiber == null)
            {
                var message = @"IsFiberContainedInAnyTraceWithBase: Left fiber not found";
                LogFile.AppendLine(message);
                return message;
            }
            var leftNodeId = leftFiber.NodeId1;

            var rightFiber = Fibers.FirstOrDefault(f => f.NodeId1 == nodeId);
            if (rightFiber == null)
            {
                var message = @"IsFiberContainedInAnyTraceWithBase: Right fiber not found";
                LogFile.AppendLine(message);
                return message;
            }
            var rightNodeId = rightFiber.NodeId2;

            Fibers.Remove(leftFiber);
            Fibers.Remove(rightFiber);
            Fibers.Add(new Fiber() { FiberId = detourFiberId, NodeId1 = leftNodeId, NodeId2 = rightNodeId });

            var node = Nodes.FirstOrDefault(n => n.NodeId == nodeId);
            if (node == null)
            {
                var message = $@"RemoveNodeWithAllHis: Node {nodeId.First6()} not found";
                LogFile.AppendLine(message);
                return message;
            }

            Nodes.Remove(node);
            return null;
        }

        private string RemoveNodeOnEdgeWhereNoTraces(Guid nodeId)
        {
            do
            {
                var node = Nodes.First(n => n.NodeId == nodeId);
                var fiber = Fibers.First(f => f.NodeId1 == nodeId || f.NodeId2 == nodeId);
                var neighbourId = fiber.NodeId1 == nodeId ? fiber.NodeId2 : fiber.NodeId1;

                Fibers.Remove(fiber);
                Equipments.RemoveAll(e => e.NodeId == nodeId);
                Nodes.Remove(node);

                nodeId = neighbourId;
            }
            while (IsAdjustmentPoint(nodeId));

            return null;
        }

        private bool IsAdjustmentPoint(Guid nodeId)
        {
            return Equipments.FirstOrDefault(e => e.NodeId == nodeId && e.Type == EquipmentType.AdjustmentPoint) != null;
        }

        public string RemoveNodeWithAllHis(Guid nodeId)
        {
            Fibers.RemoveAll(f => f.NodeId1 == nodeId || f.NodeId2 == nodeId);
            Equipments.RemoveAll(e => e.NodeId == nodeId);
            var node = Nodes.FirstOrDefault(n => n.NodeId == nodeId);
            if (node != null)
            {
                Nodes.Remove(node);
                return null;
            }

            var message = $@"RemoveNodeWithAllHis: Node {nodeId.First6()} not found";
            LogFile.AppendLine(message);
            return message;

        }

        private void CreateDetourIfAbsent(Trace trace, Guid fiberId, int idxInTrace)
        {
            var nodeBefore = trace.NodeIds[idxInTrace - 1];
            var nodeAfter = trace.NodeIds[idxInTrace + 1];

            if (!Fibers.Any(f => f.NodeId1 == nodeBefore && f.NodeId2 == nodeAfter
                                  || f.NodeId2 == nodeBefore && f.NodeId1 == nodeAfter))
                Fibers.Add(new Fiber() { FiberId = fiberId, NodeId1 = nodeBefore, NodeId2 = nodeAfter });
        }

      

        public bool IsNodeContainedInAnyTraceWithBase(Guid nodeId)
        {
            return Traces.Any(t => t.HasAnyBaseRef && t.NodeIds.Contains(nodeId));
        }
        public bool IsNodeLastForAnyTrace(Guid nodeId)
        {
            return Traces.Any(t => t.NodeIds.Last() == nodeId);
        }
        #endregion

        #region Fiber
        public string Apply(FiberAdded e)
        {
            Fibers.Add(Mapper.Map<Fiber>(e));
            return null;
        }

        public string Apply(FiberUpdated source)
        {
            var fiber = Fibers.FirstOrDefault(f => f.FiberId == source.Id);
            if (fiber != null)
            {
                fiber.UserInputedLength = source.UserInputedLength;
                return null;
            }

            var message = $@"FiberUpdated: Fiber {source.Id.First6()} not found";
            LogFile.AppendLine(message);
            return message;
        }

        public string Apply(FiberRemoved e)
        {
            var fiber = Fibers.FirstOrDefault(f => f.FiberId == e.FiberId);
            if (fiber != null)
            {
                this.RemoveFiberUptoRealNodesNotPoints(fiber);
                return null;
            }

            var message = $@"FiberRemoved: Fiber {e.FiberId.First6()} not found";
            LogFile.AppendLine(message);
            return message;
        }



        #endregion

        #region Equipment
        public string Apply(EquipmentIntoNodeAdded e)
        {
            var node = Nodes.FirstOrDefault(n => n.NodeId == e.NodeId);
            if (node == null)
            {
                var message = $@"EquipmentIntoNodeAdded: Node {e.NodeId.First6()} not found";
                LogFile.AppendLine(message);
                return message;
            }
            Equipments.Add(new Equipment() { EquipmentId = e.EquipmentId, Type = e.Type, NodeId = e.NodeId });
            return null;
        }

        public string Apply(EquipmentAtGpsLocationAdded e)
        {
            Nodes.Add(new Node() { NodeId = e.NodeId, Position = new PointLatLng(e.Latitude, e.Longitude) });

            Equipments.Add(new Equipment() { EquipmentId = e.RequestedEquipmentId, Type = e.Type, NodeId = e.NodeId });
            if (e.EmptyNodeEquipmentId != Guid.Empty)
                Equipments.Add(new Equipment() { EquipmentId = e.EmptyNodeEquipmentId, Type = EquipmentType.EmptyNode, NodeId = e.NodeId });

            return null;
        }
        public string Apply(EquipmentAtGpsLocationWithNodeTitleAdded e)
        {
            Nodes.Add(new Node() { NodeId = e.NodeId, Position = new PointLatLng(e.Latitude, e.Longitude), Title = e.Title, Comment = e.Comment });

            if (e.RequestedEquipmentId != Guid.Empty)
                Equipments.Add(new Equipment() { EquipmentId = e.RequestedEquipmentId, Type = e.Type, NodeId = e.NodeId });
            if (e.EmptyNodeEquipmentId != Guid.Empty)
                Equipments.Add(new Equipment() { EquipmentId = e.EmptyNodeEquipmentId, Type = EquipmentType.EmptyNode, NodeId = e.NodeId });

            return null;
        }

        public string Apply(EquipmentUpdated cmd)
        {
            var equipment = Equipments.FirstOrDefault(e => e.EquipmentId == cmd.EquipmentId);
            if (equipment != null)
            {
                equipment.Title = cmd.Title;
                equipment.Type = cmd.Type;
                equipment.CableReserveLeft = cmd.CableReserveLeft;
                equipment.CableReserveRight = cmd.CableReserveRight;
                equipment.Comment = cmd.Comment;
                return null;
            }

            var message = $@"EquipmentUpdated: Equipment {cmd.EquipmentId.First6()} not found";
            LogFile.AppendLine(message);
            return message;
        }

        public string Apply(EquipmentRemoved cmd)
        {
            var equipment = Equipments.FirstOrDefault(e => e.EquipmentId == cmd.EquipmentId);
            if (equipment == null)
            {
                var message = $@"EquipmentRemoved: Equipment {cmd.EquipmentId.First6()} not found";
                LogFile.AppendLine(message);
                return message;
            }

            var emptyEquipment = Equipments.FirstOrDefault(e => e.NodeId == equipment.NodeId && e.Type == EquipmentType.EmptyNode);
            if (emptyEquipment == null)
            {
                var message = $@"EquipmentRemoved: There is no empty equipment in node {equipment.NodeId.First6()}";
                LogFile.AppendLine(message);
                return message;
            }

            var traces = Traces.Where(t => t.EquipmentIds.Contains(equipment.EquipmentId));
            foreach (var trace in traces)
            {
                var index = trace.EquipmentIds.FindIndex(e => e == equipment.EquipmentId);
                trace.EquipmentIds.Insert(index, emptyEquipment.EquipmentId);
            }
            Equipments.Remove(equipment);
            return null;

        }
        #endregion

        #region Rtu
        public string Apply(RtuAtGpsLocationAdded e)
        {
            Nodes.Add(new Node() { NodeId = e.NodeId, Position = new PointLatLng(e.Latitude, e.Longitude) });
            Rtus.Add(Mapper.Map<Rtu>(e));
            return null;
        }

        public string Apply(RtuUpdated e)
        {
            var rtu = Rtus.FirstOrDefault(r => r.Id == e.RtuId);
            if (rtu != null)
            {
                rtu.Title = e.Title;
                rtu.Comment = e.Comment;

                var nodeOfRtu = Nodes.First(n => n.NodeId == rtu.NodeId);
                nodeOfRtu.Position = e.Position;
                return null;
            }

            var message = $@"RtuUpdated: RTU {e.RtuId.First6()} not found";
            LogFile.AppendLine(message);
            return message;
        }

        public string Apply(RtuRemoved cmd)
        {
            var rtu = Rtus.FirstOrDefault(r => r.Id == cmd.RtuId);
            if (rtu != null)
            {
                var nodeId = rtu.NodeId;
                Traces.RemoveAll(t => t.RtuId == rtu.Id);
                Rtus.Remove(rtu);
                RemoveNodeWithAllHis(nodeId);
                return null;
            }

            var message = $@"RtuRemoved: RTU {cmd.RtuId.First6()} not found";
            LogFile.AppendLine(message);
            return message;
        }


        #endregion

        #region Trace
        public string Apply(TraceAdded e)
        {
            Traces.Add(Mapper.Map<Trace>(e));
            return null;
        }

        public string Apply(TraceCleaned e)
        {
            var trace = Traces.FirstOrDefault(t => t.TraceId == e.TraceId);
            if (trace != null)
            {
                Traces.Remove(trace);
                return null;
            }
            var message = $@"TraceCleaned: Trace {e.TraceId} not found";
            LogFile.AppendLine(message);
            return message;
        }

        public string Apply(TraceRemoved e)
        {
            var trace = Traces.FirstOrDefault(t => t.TraceId == e.TraceId);
            if (trace != null)
            {
                Traces.Remove(trace);
                return null;
            }
            var message = $@"TraceRemoved: Trace {e.TraceId} not found";
            LogFile.AppendLine(message);
            return message;
        }

        public string Apply(TraceAttached e)
        {
            var trace = Traces.FirstOrDefault(t => t.TraceId == e.TraceId);
            if (trace == null)
            {
                var message = $@"TraceAttached: Trace {e.TraceId} not found";
                LogFile.AppendLine(message);
                return message;
            }

            trace.OtauPort = e.OtauPortDto;
            return null;
        }

        public string Apply(TraceDetached e)
        {
            var trace = Traces.FirstOrDefault(t => t.TraceId == e.TraceId);
            if (trace == null)
            {
                var message = $@"TraceDetached: Trace {e.TraceId} not found";
                LogFile.AppendLine(message);
                return message;
            }

            trace.OtauPort = null;
            return null;
        }


        #endregion

        #region JustEchosOfCmdsSentToRtu
        public string Apply(BaseRefAssigned cmd)
        {
            var trace = Traces.FirstOrDefault(t => t.TraceId == cmd.TraceId);
            if (trace != null)
            {
                foreach (var baseRefEvSo in cmd.BaseRefs)
                {
                    BaseRefs.Add(baseRefEvSo);

                    if (baseRefEvSo.BaseRefType == BaseRefType.Precise)
                    {
                        trace.PreciseId = baseRefEvSo.Id;
                        trace.PreciseDuration = baseRefEvSo.Duration;
                    }
                    if (baseRefEvSo.BaseRefType == BaseRefType.Fast)
                    {
                        trace.FastId = baseRefEvSo.Id;
                        trace.FastDuration = baseRefEvSo.Duration;
                    }
                    if (baseRefEvSo.BaseRefType == BaseRefType.Additional)
                    {
                        trace.AdditionalId = baseRefEvSo.Id;
                        trace.AdditionalDuration = baseRefEvSo.Duration;
                    }
                }
                if (!trace.HasEnoughBaseRefsToPerformMonitoring)
                    trace.IsIncludedInMonitoringCycle = false;
                return null;
            }
            var message = $@"BaseRefAssigned: Trace {cmd.TraceId} not found";
            LogFile.AppendLine(message);
            return message;
        }

        public string Apply(RtuInitialized e)
        {
            var rtu = Rtus.FirstOrDefault(r => r.Id == e.Id);
            if (rtu != null)
            {
                Mapper.Map(e, rtu);
                return null;
            }
            var message = $@"RtuInitialized: RTU {e.Id.First6()} not found";
            LogFile.AppendLine(message);
            return message;
        }

        public string Apply(MonitoringSettingsChanged cmd)
        {
            var rtu = Rtus.FirstOrDefault(r => r.Id == cmd.RtuId);
            if (rtu != null)
            {
                rtu.MonitoringState = cmd.IsMonitoringOn ? MonitoringState.On : MonitoringState.Off;
                rtu.FastSave = cmd.FastSave;
                rtu.PreciseMeas = cmd.PreciseMeas;
                rtu.PreciseSave = cmd.PreciseSave;
                return null;
            }

            var message = $@"MonitoringSettingsChanged: RTU {cmd.RtuId.First6()} not found";
            LogFile.AppendLine(message);
            return message;

        }
        public string Apply(MonitoringStarted e)
        {
            var rtu = Rtus.FirstOrDefault(r => r.Id == e.RtuId);
            if (rtu != null)
            {
                rtu.MonitoringState = MonitoringState.On;
                return null;
            }
            var message = $@"MonitoringStarted: RTU {e.RtuId.First6()} not found";
            LogFile.AppendLine(message);
            return message;
        }
        public string Apply(MonitoringStopped e)
        {
            var rtu = Rtus.FirstOrDefault(r => r.Id == e.RtuId);
            if (rtu != null)
            {
                rtu.MonitoringState = MonitoringState.Off;
                return null;
            }
            var message = $@"MonitoringStopped: RTU {e.RtuId.First6()} not found";
            LogFile.AppendLine(message);
            return message;
        }

        public string Apply(MeasurementAdded e)
        {
            Measurements.Add(Mapper.Map<Measurement>(e));
            return null;
        }

        public string Apply(MeasurementUpdated e)
        {
            var destination = Measurements.First(f => f.SorFileId == e.SorFileId);
            Mapper.Map(e, destination);
            return null;
        }

        public string Apply(NetworkEventAdded e)
        {
            var networkEvent = Mapper.Map<NetworkEvent>(e);
            var rtu = Rtus.First(r => r.Id == e.RtuId);
            rtu.MainChannelState = e.MainChannelState;
            rtu.ReserveChannelState = e.ReserveChannelState;
            NetworkEvents.Add(networkEvent);
            return null;
        }

        public string Apply(BopNetworkEventAdded e)
        {
            BopNetworkEvents.Add(Mapper.Map<BopNetworkEvent>(e));
            return null;
        }

        public string Apply(ResponsibilitiesChanged e)
        {
            this.ChangeResponsibilities(e);
            return null;
        }
        #endregion
    }
}