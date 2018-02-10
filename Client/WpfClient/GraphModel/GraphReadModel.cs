using System;
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

    }
}