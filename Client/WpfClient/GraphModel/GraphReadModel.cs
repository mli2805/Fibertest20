using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Autofac;
using Caliburn.Micro;
using GMap.NET;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
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
        private readonly IniFile _iniFile;

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

        public List<GraphVisibilityLevelItem> GraphVisibilityItems { get; set; } 
        private GraphVisibilityLevelItem _selectedGraphVisibilityItem;

        public GraphVisibilityLevelItem SelectedGraphVisibilityItem
        {
            get => _selectedGraphVisibilityItem;
            set
            {
                if (value == _selectedGraphVisibilityItem) return;
                _selectedGraphVisibilityItem = value;
                _iniFile.Write(IniSection.Miscellaneous, IniKey.GraphVisibilityLevel, SelectedGraphVisibilityItem.Level.ToString());
                NotifyOfPropertyChange();
            }
        }

        public GraphReadModel(ILifetimeScope globalScope, IniFile iniFile,   
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
            _iniFile = iniFile;
            Nodes = new ObservableCollection<NodeVm>();
            Fibers = new ObservableCollection<FiberVm>();
            Rtus = new ObservableCollection<RtuVm>();
            Equipments = new ObservableCollection<EquipmentVm>();
            Traces = new ObservableCollection<TraceVm>();

            GraphVisibilityItems = GraphVisibilityExt.GetComboboxItems();
            var levelString = iniFile.Read(IniSection.Miscellaneous, IniKey.GraphVisibilityLevel,
                GraphVisibilityLevel.AllDetails.ToString());
            if (!Enum.TryParse(levelString, out GraphVisibilityLevel level))
                level = GraphVisibilityLevel.AllDetails;
            SetGraphVisibility(level);

            Zoom = iniFile.Read(IniSection.Map, IniKey.Zoom, 7);
            ToCenter = new PointLatLng()
            {
                Lat = iniFile.Read(IniSection.Map, IniKey.CenterLatitude, 53.856),
                Lng = iniFile.Read(IniSection.Map, IniKey.CenterLongitude, 27.49),
            };
        }

        public void SetGraphVisibility(GraphVisibilityLevel level)
        {
            SelectedGraphVisibilityItem =
                GraphVisibilityItems.First(i => i.Level == level);
            _iniFile.Write(IniSection.Miscellaneous, IniKey.GraphVisibilityLevel, level.ToString());
        }
        public void PlaceRtuIntoScreenCenter(Guid rtuId)
        {
            var nodeVm = Rtus.First(r => r.Id == rtuId).Node;
            nodeVm.IsHighlighted = true;
            ToCenter = nodeVm.Position;
        }

        public void ExtinguishNode()
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

    }
}