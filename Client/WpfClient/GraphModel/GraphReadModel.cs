﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Autofac;
using AutoMapper;
using Caliburn.Micro;
using GMap.NET;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Client
{
    public class GraphReadModel : PropertyChangedBase
    {
        public CommonStatusBarViewModel CommonStatusBarViewModel { get; }
        public GrmNodeRequests GrmNodeRequests { get; }
        public GrmEquipmentRequests GrmEquipmentRequests { get; }
        public GrmFiberRequests GrmFiberRequests { get; }
        public GrmFiberWithNodesRequest GrmFiberWithNodesRequest { get; }
        public GrmRtuRequests GrmRtuRequests { get; }
        public IWindowManager WindowManager { get; }
        public ReadModel ReadModel { get; }
        public readonly ILifetimeScope GlobalScope;
        private readonly IMyLog _logFile;

        public ObservableCollection<NodeVm> Nodes { get; }
        public ObservableCollection<FiberVm> Fibers { get; }
        public ObservableCollection<RtuVm> Rtus { get; }
        public ObservableCollection<EquipmentVm> Equipments { get; }
        public ObservableCollection<TraceVm> Traces { get; }

        private PointLatLng _currentMousePosition;
        public PointLatLng CurrentMousePosition
        {
            get => _currentMousePosition;
            set
            {
                if (value.Equals(_currentMousePosition)) return;
                _currentMousePosition = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(CurrentMousePositionString));
            }
        }

        public int Zoom { get; set; } 

        private PointLatLng _toCenter;
        public PointLatLng ToCenter
        {
            get => _toCenter;
            set
            {
                if (value.Equals(_toCenter)) return;
                _toCenter = value;
                NotifyOfPropertyChange();
                CenterForIni = value;
            }
        }

        public PointLatLng CenterForIni { get; set; }

        public string CurrentMousePositionString => CurrentMousePosition.ToDetailedString(CurrentGpsInputMode);
        public GpsInputMode CurrentGpsInputMode = GpsInputMode.DegreesMinutesAndSeconds;

        public List<string> GraphVisibilityLevels { get; set; }
        public string SelectedGraphVisibilityLevel { get; set; }

        private void InitilizeVisibility()
        {
            GraphVisibilityLevels = new List<string>() { Resources.SID_Rtu, Resources.SID_Lines, Resources.SID_Equip, Resources.SID_Nodes, Resources.SID_All };
            SelectedGraphVisibilityLevel = GraphVisibilityLevels.Last();
        }

        public GraphReadModel(ILifetimeScope globalScope, IniFile iniFile, IMyLog logFile,  
            CommonStatusBarViewModel commonStatusBarViewModel,
            GrmNodeRequests grmNodeRequests, GrmEquipmentRequests grmEquipmentRequests, 
            GrmFiberRequests grmFiberRequests, GrmFiberWithNodesRequest grmFiberWithNodesRequest,
             GrmRtuRequests grmRtuRequests,
            
            IWindowManager windowManager, ReadModel readModel)
        {
            CommonStatusBarViewModel = commonStatusBarViewModel;
            GrmNodeRequests = grmNodeRequests;
            GrmEquipmentRequests = grmEquipmentRequests;
            GrmFiberRequests = grmFiberRequests;
            GrmFiberWithNodesRequest = grmFiberWithNodesRequest;
            GrmRtuRequests = grmRtuRequests;
            WindowManager = windowManager;
            ReadModel = readModel;
            GlobalScope = globalScope;
            _logFile = logFile;
            Nodes = new ObservableCollection<NodeVm>();
            Fibers = new ObservableCollection<FiberVm>();
            Rtus = new ObservableCollection<RtuVm>();
            Equipments = new ObservableCollection<EquipmentVm>();
            Traces = new ObservableCollection<TraceVm>();

            InitilizeVisibility();
            Zoom = iniFile.Read(IniSection.Map, IniKey.Zoom, 7);
            ToCenter = new PointLatLng()
            {
                Lat = iniFile.Read(IniSection.Map, IniKey.CenterLatitude, 53.856),
                Lng = iniFile.Read(IniSection.Map, IniKey.CenterLongitude, 27.49),
            };
        }

        public void PlaceRtuIntoScreenCenter(Guid rtuId)
        {
            var nodeVm = Rtus.First(r => r.Id == rtuId).Node;
            nodeVm.IsHighlighted = true;
            ToCenter = nodeVm.Position;
        }

        public void Extinguish()
        {
            var nodeVm = Nodes.FirstOrDefault(n => n.IsHighlighted);
            if (nodeVm != null)
                nodeVm.IsHighlighted = false;
        }

        public void ChangeTraceColor(Guid traceId, List<Guid> nodes, FiberState state)
        {
            var fiberIds = this.GetFibersByNodes(nodes);
            foreach (var fiberId in fiberIds)
            {
                var fiberVm = Fibers.First(f => f.Id == fiberId);
                if (state != FiberState.NotInTrace)
                    fiberVm.SetState(traceId, state);
                else
                    fiberVm.RemoveState(traceId);
            }
        }

        public void Apply(NodeHighlighted evnt)
        {
            var nodeVm = Nodes.First(n => n.Id == evnt.NodeId);
            nodeVm.IsHighlighted = true;
        }


        #region Node

        public void Apply(NodeIntoFiberAdded evnt)
        {
            var nodeVm = new NodeVm()
            {
                Id = evnt.Id,
                Position = new PointLatLng(evnt.Position.Lat, evnt.Position.Lng),
                
                State = FiberState.Ok,
                Type = evnt.InjectionType,
            };
            Nodes.Add(nodeVm);
            Equipments.Add(new EquipmentVm() { Id = evnt.EquipmentId, Type = evnt.InjectionType, Node = nodeVm });

            var fiberForDeletion = Fibers.First(f => f.Id == evnt.FiberId);
            AddTwoFibersToNewNode(evnt, fiberForDeletion);
            FixTracesWhichContainedOldFiber(evnt);
            Fibers.Remove(fiberForDeletion);
        }

        private void FixTracesWhichContainedOldFiber(NodeIntoFiberAdded e)
        {
            foreach (var trace in Traces)
            {
                int idx;
                while ((idx = GetFiberIndexInTrace(trace, Fibers.First(f => f.Id == e.FiberId))) != -1)
                {
                    trace.Nodes.Insert(idx + 1, e.Id);
                }
            }
        }
        private int GetFiberIndexInTrace(TraceVm trace, FiberVm fiber)
        {
            var idxInTrace1 = trace.Nodes.IndexOf(fiber.Node1.Id);
            if (idxInTrace1 == -1)
                return -1;
            var idxInTrace2 = trace.Nodes.IndexOf(fiber.Node2.Id);
            if (idxInTrace2 == -1)
                return -1;
            if (idxInTrace2 - idxInTrace1 == 1)
                return idxInTrace1;
            if (idxInTrace1 - idxInTrace2 == 1)
                return idxInTrace2;
            return -1;
        }

        private void AddTwoFibersToNewNode(NodeIntoFiberAdded e, FiberVm oldFiberVm)
        {
            NodeVm node1 = Fibers.First(f => f.Id == e.FiberId).Node1;
            NodeVm node2 = Fibers.First(f => f.Id == e.FiberId).Node2;

            Fibers.Add(new FiberVm() { Id = e.NewFiberId1, Node1 = node1, Node2 = Nodes.First(n => n.Id == e.Id), States = oldFiberVm.States });
            Fibers.Add(new FiberVm() { Id = e.NewFiberId2, Node1 = Nodes.First(n => n.Id == e.Id), Node2 = node2, States = oldFiberVm.States });
        }

        public void Apply(NodeMoved evnt)
        {
            var nodeVm = Nodes.FirstOrDefault(n => n.Id == evnt.NodeId);
            if (nodeVm == null)
                return;
            nodeVm.Position = new PointLatLng(evnt.Latitude, evnt.Longitude);
        }

        public void Apply(NodeUpdated evnt)
        {
            var nodeVm = Nodes.FirstOrDefault(n => n.Id == evnt.Id);
            if (nodeVm == null)
                return;
            nodeVm.Title = evnt.Title;
            nodeVm.Comment = evnt.Comment;
        }

        public void Apply(NodeRemoved evnt)
        {
            foreach (var traceVm in Traces.Where(t => t.Nodes.Contains(evnt.Id)))
                ExcludeNodeFromTrace(traceVm, evnt.TraceWithNewFiberForDetourRemovedNode[traceVm.Id], evnt.Id);
            RemoveNodeWithAllHis(evnt.Id);
        }

        private void ExcludeNodeFromTrace(TraceVm traceVm, Guid fiberId, Guid nodeId)
        {
            var idxInTrace = traceVm.Nodes.IndexOf(nodeId);
            CreateDetourIfAbsent(traceVm, fiberId, idxInTrace);

            traceVm.Nodes.RemoveAt(idxInTrace);
        }

        private void RemoveNodeWithAllHis(Guid nodeId)
        {
            foreach (var fiberVm in Fibers.Where(f => f.Node1.Id == nodeId || f.Node2.Id == nodeId).ToList())
                Fibers.Remove(fiberVm);
            foreach (var equipmentVm in Equipments.Where(e => e.Node.Id == nodeId).ToList())
                Equipments.Remove(equipmentVm);

            var nodeVm = Nodes.FirstOrDefault(n => n.Id == nodeId);
            if (nodeVm != null)
                Nodes.Remove(nodeVm);
            else _logFile.AppendLine($@"NodeVm {nodeId.First6()} not found");
        }
        private void CreateDetourIfAbsent(TraceVm traceVm, Guid fiberId, int idxInTrace)
        {
            var nodeBefore = traceVm.Nodes[idxInTrace - 1];
            var nodeAfter = traceVm.Nodes[idxInTrace + 1];

            if (!Fibers.Any(f => f.Node1.Id == nodeBefore && f.Node2.Id == nodeAfter
                                 || f.Node2.Id == nodeBefore && f.Node1.Id == nodeAfter))
            {
                var fiberVm = new FiberVm()
                {
                    Id = fiberId,
                    Node1 = Nodes.First(m => m.Id == nodeBefore),
                    Node2 = Nodes.First(m => m.Id == nodeAfter),
                };
                Fibers.Add(fiberVm);
                fiberVm.SetState(traceVm.Id, traceVm.State);
            }
        }
        #endregion

        #region Fiber
        public void Apply(FiberAdded evnt)
        {
            Fibers.Add(new FiberVm()
            {
                Id = evnt.Id,
                Node1 = Nodes.First(m => m.Id == evnt.Node1),
                Node2 = Nodes.First(m => m.Id == evnt.Node2),
            });
        }

        public void Apply(FiberRemoved evnt)
        {
            Fibers.Remove(Fibers.First(f => f.Id == evnt.Id));
        }
        #endregion

        #region RTU
        public void Apply(RtuAtGpsLocationAdded evnt)
        {
            var nodeVm = new NodeVm()
            {
                Id = evnt.NodeId,
                State = FiberState.Ok,
                Type = EquipmentType.Rtu,
                Position = new PointLatLng(evnt.Latitude, evnt.Longitude),
            };
            Nodes.Add(nodeVm);

            var rtuVm = new RtuVm() { Id = evnt.Id, Node = nodeVm };
            Rtus.Add(rtuVm);
        }

        public void Apply(RtuUpdated evnt)
        {
            var rtu = Rtus.FirstOrDefault(r => r.Id == evnt.Id);
            if (rtu == null)
                return;
            rtu.Title = evnt.Title;
            rtu.Node.Title = evnt.Title;
        }

        public void Apply(RtuRemoved evnt)
        {
            var rtuVm = Rtus.FirstOrDefault(r => r.Id == evnt.Id);
            if (rtuVm == null) return;
            Guid nodeId = rtuVm.Node.Id;
            foreach (var t in Traces.Where(t => t.RtuId == rtuVm.Id).ToList())
            {
                Apply(new TraceCleaned() {Id = t.Id});
                Traces.Remove(t);
            }
            Rtus.Remove(rtuVm);
            RemoveNodeWithAllHis(nodeId);
        }
        #endregion

        #region Equipment
        public void Apply(EquipmentAtGpsLocationAdded evnt)
        {
            var nodeVm = new NodeVm()
            {
                Id = evnt.NodeId,
                State = FiberState.Ok,
                Type = evnt.Type,
                Position = new PointLatLng(evnt.Latitude, evnt.Longitude)
            };
            Nodes.Add(nodeVm);

            Equipments.Add(new EquipmentVm() { Id = evnt.RequestedEquipmentId, Node = nodeVm, Type = evnt.Type });
            if (evnt.EmptyNodeEquipmentId != Guid.Empty)
                Equipments.Add(new EquipmentVm() { Id = evnt.EmptyNodeEquipmentId, Node = nodeVm, Type = EquipmentType.EmptyNode });
        }

        public void Apply(EquipmentIntoNodeAdded evnt)
        {
            var nodeVm = Nodes.First(n => n.Id == evnt.NodeId);
            Equipments.Add(new EquipmentVm() { Id = evnt.Id, Node = nodeVm, Type = evnt.Type });
            nodeVm.Type = evnt.Type;
        }

        public void Apply(EquipmentUpdated evnt)
        {
            var equipmentVm = Equipments.First(e => e.Id == evnt.Id);
            IMapper mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingEventToVm>()).CreateMapper();
            mapper.Map(evnt, equipmentVm);
            var nodeVm = equipmentVm.Node;
            nodeVm.Type = evnt.Type;
        }

        public void Apply(EquipmentRemoved evnt)
        {
            var equipmentVm = Equipments.FirstOrDefault(e => e.Id == evnt.Id);
            if (equipmentVm == null)
            {
                var message = $@"EquipmentRemoved: Equipment {evnt.Id.First6()} not found";
                _logFile.AppendLine(message);
                return;
            }
            var nodeVm = equipmentVm.Node;

            Equipments.Remove(equipmentVm);

            var majorEquipmentInNode = Equipments.Where(e => e.Node.Id == nodeVm.Id).Max(e=>e.Type);
            nodeVm.Type = majorEquipmentInNode;
        }
        #endregion

        #region Trace
        public void Apply(TraceAdded evnt)
        {
            IMapper mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingEventToVm>()).CreateMapper();
            var traceVm = mapper.Map<TraceVm>(evnt);
            Traces.Add(traceVm);

            ApplyTraceStateToFibers(traceVm);
        }

        private IEnumerable<FiberVm> GetTraceFibersByNodes(List<Guid> nodes)
        {
            for (int i = 1; i < nodes.Count; i++)
                yield return GetFiberBetweenNodes(nodes[i - 1], nodes[i]);
        }

        private FiberVm GetFiberBetweenNodes(Guid node1, Guid node2)
        {
            return Fibers.First(
                f => f.Node1.Id == node1 && f.Node2.Id == node2 ||
                     f.Node1.Id == node2 && f.Node2.Id == node1);
        }

        public void Apply(TraceCleaned evnt)
        {
            var traceVm = Traces.First(t => t.Id == evnt.Id);
            GetTraceFibersByNodes(traceVm.Nodes).ToList().ForEach(f => f.RemoveState(evnt.Id));
            Traces.Remove(traceVm);
        }

        public void Apply(TraceRemoved evnt)
        {
            var traceVm = Traces.First(t => t.Id == evnt.Id);
            var traceFibers = GetTraceFibersByNodes(traceVm.Nodes).ToList();
            foreach (var fiberVm in traceFibers)
            {
                fiberVm.RemoveState(evnt.Id);
                if (fiberVm.State == FiberState.NotInTrace)
                    Fibers.Remove(fiberVm);
            }
            foreach (var nodeId in traceVm.Nodes)
            {
                if (Rtus.Any(r => r.Node.Id == nodeId) ||
                    Fibers.Any(f => f.Node1.Id == nodeId || f.Node2.Id == nodeId))
                    continue;
                var nodeVm = Nodes.First(n => n.Id == nodeId);
                Nodes.Remove(nodeVm);
            }
            Traces.Remove(traceVm);
        }

        public void Apply(TraceAttached evnt)
        {
            var traceVm = Traces.First(t => t.Id == evnt.TraceId);
            traceVm.State = FiberState.Unknown;
            traceVm.Port = evnt.OtauPortDto.OpticalPort;
            ApplyTraceStateToFibers(traceVm);
        }

        public void Apply(TraceDetached evnt)
        {
            var traceVm = Traces.First(t => t.Id == evnt.TraceId);
            traceVm.State = FiberState.NotJoined;
            traceVm.Port = 0;
            ApplyTraceStateToFibers(traceVm);
        }

        private void ApplyTraceStateToFibers(TraceVm traceVm)
        {
            foreach (var fiberVm in GetTraceFibersByNodes(traceVm.Nodes))
                fiberVm.SetState(traceVm.Id, traceVm.State);
        }
        #endregion
    }
}