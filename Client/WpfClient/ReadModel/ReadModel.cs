using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Algorithms;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Client
{
    public class ReadModel : PropertyChangedBase
    {
        public readonly IMyLog LogFile;

        private readonly IMapper _mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingEventToDomainModelProfile>()).CreateMapper();

        public List<Node> Nodes { get; } = new List<Node>();
        public List<Fiber> Fibers { get; } = new List<Fiber>();
        public List<Equipment> Equipments { get; } = new List<Equipment>();
        public List<Rtu> Rtus { get; } = new List<Rtu>();
        public List<Trace> Traces { get; } = new List<Trace>();
        public List<Otau> Otaus { get; } = new List<Otau>();

        public int JustForNotification { get; set; }

        public ReadModel(IMyLog logFile)
        {
            LogFile = logFile;
        }

        #region Node
        public void Apply(NodeAdded e)
        {
            Node node = _mapper.Map<Node>(e);
            Nodes.Add(node);
        }

        public void Apply(NodeIntoFiberAdded e)
        {
            Nodes.Add(new Node() { Id = e.Id, Latitude = e.Position.Lat, Longitude = e.Position.Lng});
            Equipments.Add(new Equipment(){Id = e.EquipmentId, Type = e.IsAdjustmentPoint ? EquipmentType.AdjustmentPoint : EquipmentType.EmptyNode, NodeId = e.Id});
            AddTwoFibersToNewNode(e);
            FixTracesWhichContainedOldFiber(e);
            Fibers.Remove(Fibers.First(f => f.Id == e.FiberId));
        }
        private void FixTracesWhichContainedOldFiber(NodeIntoFiberAdded e)
        {
            foreach (var trace in Traces)
            {
                int idx;
                while ((idx = Topo.GetFiberIndexInTrace(trace, Fibers.First(f => f.Id == e.FiberId))) != -1)
                {
                    trace.Nodes.Insert(idx + 1, e.Id); // GPS location добавляется во все трассы
                    trace.Equipments.Insert(idx + 1, Guid.Empty); 
                }
            }
        }
        private void AddTwoFibersToNewNode(NodeIntoFiberAdded e)
        {
            Guid nodeId1 = Fibers.First(f => f.Id == e.FiberId).Node1;
            Guid nodeId2 = Fibers.First(f => f.Id == e.FiberId).Node2;

            Fibers.Add(new Fiber() { Id = e.NewFiberId1, Node1 = nodeId1, Node2 = e.Id });
            Fibers.Add(new Fiber() { Id = e.NewFiberId2, Node1 = e.Id, Node2 = nodeId2 });
        }

        public void Apply(NodeUpdated source)
        {
            Node destination = Nodes.First(n => n.Id == source.Id);
            _mapper.Map(source, destination);
        }

        public void Apply(NodeMoved newLocation)
        {
            Node oldLocation = Nodes.FirstOrDefault(n => n.Id == newLocation.Id);
            if (oldLocation != null)
                _mapper.Map(newLocation, oldLocation);
        }

        public void Apply(NodeRemoved e)
        {
            foreach (var trace in Traces.Where(t => t.Nodes.Contains(e.Id)))
                ExcludeNodeFromTrace(trace, e.TraceWithNewFiberForDetourRemovedNode[trace.Id], e.Id);

            RemoveNodeWithAllHis(e.Id);
        }

        private void ExcludeNodeFromTrace(Trace trace, Guid fiberId, Guid nodeId)
        {
            var idxInTrace = trace.Nodes.IndexOf(nodeId);
            CreateDetourIfAbsent(trace, fiberId, idxInTrace);

            trace.Equipments.RemoveAt(idxInTrace);
            trace.Nodes.RemoveAt(idxInTrace);
        }

        private void RemoveNodeWithAllHis(Guid nodeId)
        {
            Fibers.RemoveAll(f => f.Node1 == nodeId || f.Node2 == nodeId);
            Equipments.RemoveAll(e => e.NodeId == nodeId);
            var node = Nodes.FirstOrDefault(n => n.Id == nodeId);
            if (node != null)
                Nodes.Remove(node);
            else
                LogFile.AppendLine($@"Node {nodeId.First6()} not found");
        }

        private void CreateDetourIfAbsent(Trace trace, Guid fiberId, int idxInTrace)
        {
            var nodeBefore = trace.Nodes[idxInTrace - 1];
            var nodeAfter = trace.Nodes[idxInTrace + 1];

            if (!Fibers.Any(f => f.Node1 == nodeBefore && f.Node2 == nodeAfter
                               || f.Node2 == nodeBefore && f.Node1 == nodeAfter))
                Fibers.Add(new Fiber() { Id = fiberId, Node1 = nodeBefore, Node2 = nodeAfter });
        }
        #endregion

        #region Fiber
        public bool HasFiberBetween(Guid a, Guid b)
        {
            return Fibers.Any(f =>
                f.Node1 == a && f.Node2 == b ||
                f.Node1 == b && f.Node2 == a);
        }
        public void Apply(FiberAdded e)
        {
            Fibers.Add(_mapper.Map<Fiber>(e));
        }

        public void Apply(FiberUpdated source)
        {
            var destination = Fibers.First(f => f.Id == source.Id);
            _mapper.Map(source, destination);
        }

        public void Apply(FiberRemoved e)
        {
            Fibers.Remove(Fibers.First(f => f.Id == e.Id));
        }
        #endregion

        #region Equipment
        public void Apply(EquipmentIntoNodeAdded e)
        {
            Equipment equipment = _mapper.Map<Equipment>(e);
            Equipments.Add(equipment);
            foreach (var traceId in e.TracesForInsertion)
            {
                var trace = Traces.Single(t => t.Id == traceId);
                var idx = trace.Nodes.IndexOf(e.NodeId);
                trace.Equipments[idx] = e.Id;
            }
        }

        public void Apply(EquipmentAtGpsLocationAdded e)
        {
            Node node = new Node() { Id = e.NodeId, Latitude = e.Latitude, Longitude = e.Longitude };
            Nodes.Add(node);
            Equipment equipment = _mapper.Map<Equipment>(e);
            equipment.Id = e.RequestedEquipmentId;
            Equipments.Add(equipment);
            if (e.EmptyNodeEquipmentId != Guid.Empty)
            {
                Equipment emptyEquipment = _mapper.Map<Equipment>(e);
                emptyEquipment.Id = e.EmptyNodeEquipmentId;
                emptyEquipment.Type = EquipmentType.EmptyNode;
                Equipments.Add(emptyEquipment);
            }
        }

        public void Apply(EquipmentUpdated e)
        {
            var equipment = Equipments.FirstOrDefault(eq => eq.Id == e.Id);
            _mapper.Map(e, equipment);
        }

        public void Apply(EquipmentRemoved e)
        {
            var equipment = Equipments.FirstOrDefault(eq => eq.Id == e.Id);
            if (equipment == null)
            {
                var message = $@"EquipmentRemoved: Equipment {e.Id.First6()} not found";
                LogFile.AppendLine(message);
                return;
            }

            var emptyEquipment = Equipments.FirstOrDefault(eq => eq.NodeId == equipment.NodeId && eq.Type == EquipmentType.EmptyNode);
            if (emptyEquipment == null)
            {
                var message = $@"EquipmentRemoved: There is no empty equipment in node {equipment.NodeId.First6()}";
                LogFile.AppendLine(message);
                return;
            }

            var traces = Traces.Where(t => t.Equipments.Contains(e.Id)).ToList();
            foreach (var trace in traces)
            {
                var idx = trace.Equipments.IndexOf(e.Id);
                trace.Equipments[idx] = emptyEquipment.Id;
            }
            Equipments.Remove(Equipments.First(eq => eq.Id == e.Id));
        }
        #endregion

        #region RTU
        public void Apply(RtuAtGpsLocationAdded e)
        {
            Node node = new Node() { Id = e.NodeId, Latitude = e.Latitude, Longitude = e.Longitude };
            Nodes.Add(node);
            Rtu rtu = _mapper.Map<Rtu>(e);
            Rtus.Add(rtu);

        }

        public void Apply(RtuUpdated e)
        {
            var rtu = Rtus.FirstOrDefault(r => r.Id == e.Id);
            if (rtu == null)
                return;
            rtu.Title = e.Title;
            rtu.Comment = e.Comment;
        }

        public void Apply(RtuRemoved e)
        {
            var rtu = Rtus.FirstOrDefault(r => r.Id == e.Id);
            if (rtu == null)
                return;
            var nodeId = rtu.NodeId;
            Traces.RemoveAll(t => t.RtuId == rtu.Id);
            Rtus.Remove(rtu);
            RemoveNodeWithAllHis(nodeId);
        }

        public void Apply(OtauAttached e)
        {
            Otau otau = _mapper.Map<Otau>(e);
            Otaus.Add(otau);
        }

        public void Apply(OtauDetached e)
        {
            var otau = Otaus.FirstOrDefault(o => o.Id == e.Id);
            if (otau == null)
                return;
            var rtu = Rtus.FirstOrDefault(r => r.Id == e.RtuId);
            if (rtu == null)
                return;

            rtu.FullPortCount -= otau.PortCount;
            Otaus.Remove(otau);
        }

        #endregion

        #region Trace
        public void Apply(TraceAdded e)
        {
            Trace trace = _mapper.Map<Trace>(e);
            Traces.Add(trace);
        }

        public void Apply(TraceUpdated source)
        {
            var destination = Traces.First(t => t.Id == source.Id);
            _mapper.Map(source, destination);
        }

        public void Apply(TraceCleaned e)
        {
            var trace = Traces.First(t => t.Id == e.Id);
            Traces.Remove(trace);
        }

        private IEnumerable<Fiber> GetTraceFibersByNodes(List<Guid> nodes)
        {
            for (int i = 1; i < nodes.Count; i++)
                yield return GetFiberBetweenNodes(nodes[i - 1], nodes[i]);
        }

        private Fiber GetFiberBetweenNodes(Guid node1, Guid node2)
        {
            return Fibers.First(
                f => f.Node1 == node1 && f.Node2 == node2 ||
                     f.Node1 == node2 && f.Node2 == node1);
        }

        public void Apply(TraceRemoved e)
        {
            var traceVm = Traces.First(t => t.Id == e.Id);
            var traceFibers = GetTraceFibersByNodes(traceVm.Nodes).ToList();
            foreach (var fiber in traceFibers)
            {
                if (Traces.All(trace => Topo.GetFiberIndexInTrace(trace, fiber) == -1))
                    Fibers.Remove(fiber);
            }
            Traces.Remove(traceVm);
        }

        public void Apply(TraceAttached e)
        {
            var trace = Traces.First(t => t.Id == e.TraceId);
            trace.Port = e.OtauPortDto.OpticalPort;
            trace.OtauPort = e.OtauPortDto;
        }

        public void Apply(TraceDetached e)
        {
            var trace = Traces.First(t => t.Id == e.TraceId);
            trace.Port = -1;
            trace.OtauPort = null;
            trace.IsIncludedInMonitoringCycle = false;
        }

        #endregion

        #region JustEchosOfCmdsSentToRtu
        public void Apply(BaseRefAssigned e)
        {
            var trace = Traces.Single(t => t.Id == e.TraceId);

            var preciseBaseRef = e.BaseRefs.FirstOrDefault(b => b.BaseRefType == BaseRefType.Precise);
            if (preciseBaseRef != null)
            {
                trace.PreciseId = preciseBaseRef.Id;
                trace.PreciseDuration = preciseBaseRef.Duration;
            }
            var fastBaseRef = e.BaseRefs.FirstOrDefault(b => b.BaseRefType == BaseRefType.Fast);
            if (fastBaseRef != null)
            {
                trace.FastId = fastBaseRef.Id;
                trace.FastDuration = fastBaseRef.Duration;
            }
            var additionalBaseRef = e.BaseRefs.FirstOrDefault(b => b.BaseRefType == BaseRefType.Additional);
            if (additionalBaseRef != null)
            {
                trace.AdditionalId = additionalBaseRef.Id;
                trace.AdditionalDuration = additionalBaseRef.Duration;
            }
            if (!trace.HasEnoughBaseRefsToPerformMonitoring)
                trace.IsIncludedInMonitoringCycle = false;
        }

        public void Apply(RtuInitialized e)
        {
            var rtu = Rtus.First(r => r.Id == e.Id);
            InitializeRtuFirstTime(e, rtu);

            if (rtu.Serial == null)
            {
                return;
            }

            if (rtu.Serial == e.Serial)
            {
                if (rtu.OwnPortCount != e.OwnPortCount)
                {
                    // main otdr problem
                    // TODO
                    return;
                }

                if (rtu.FullPortCount != e.FullPortCount)
                {
                    // bop changes
                    // TODO
                    return;
                }

                if (rtu.FullPortCount == e.FullPortCount)
                {
                    // just re-initialization, nothing should be done?
                    rtu.Version = e.Version;
                }
            }

            if (rtu.Serial != e.Serial)
            {
                //TODO discuss and implement rtu replacement scenario
            }
        }

        private static void InitializeRtuFirstTime(RtuInitialized e, Rtu rtu)
        {
            rtu.OwnPortCount = e.OwnPortCount;
            rtu.FullPortCount = e.FullPortCount;
            rtu.Serial = e.Serial;
            rtu.MainChannel = e.MainChannel;
            rtu.MainChannelState = e.MainChannelState;
            rtu.IsReserveChannelSet = e.IsReserveChannelSet;
            if (e.IsReserveChannelSet)
                rtu.ReserveChannel = e.ReserveChannel;
            rtu.ReserveChannelState = e.ReserveChannelState;
            rtu.OtdrNetAddress = e.OtauNetAddress;
            rtu.Version = e.Version;
            rtu.MonitoringState = MonitoringState.Off;
            rtu.AcceptableMeasParams = e.AcceptableMeasParams;
        }

        public void Apply(MonitoringSettingsChanged e)
        {
            var rtu = Rtus.FirstOrDefault(r => r.Id == e.RtuId);
            if (rtu == null)
            {
                LogFile.AppendLine(@"MonitoringSettingsChanged: cant find RTU");
                return;
            }
            rtu.PreciseMeas = e.PreciseMeas;
            rtu.PreciseSave = e.PreciseSave;
            rtu.FastSave = e.FastSave;
            rtu.MonitoringState = e.IsMonitoringOn ? MonitoringState.On : MonitoringState.Off;

            foreach (var trace in Traces.Where(t => t.RtuId == e.RtuId))
            {
                trace.IsIncludedInMonitoringCycle = e.TracesInMonitoringCycle.Contains(trace.Id);
            }
        }

        public void Apply(MonitoringStarted e)
        {
            var rtu = Rtus.FirstOrDefault(r => r.Id == e.RtuId);
            if (rtu == null)
            {
                LogFile.AppendLine(@"MonitoringStarted: cant find RTU");
                return;
            }
            rtu.MonitoringState = MonitoringState.On;
        }

        public void Apply(MonitoringStopped e)
        {
            var rtu = Rtus.FirstOrDefault(r => r.Id == e.RtuId);
            if (rtu == null)
            {
                LogFile.AppendLine(@"MonitoringStopped: cant find RTU");
                return;
            }
            rtu.MonitoringState = MonitoringState.Off;
        }
        #endregion
    }
}