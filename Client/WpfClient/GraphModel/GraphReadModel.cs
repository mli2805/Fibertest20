using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Client
{
    public class GraphReadModel : PropertyChangedBase
    {
        public Map MainMap { get; set; }

        public CurrentGpsInputMode CurrentGpsInputMode { get; }
        public CommonStatusBarViewModel CommonStatusBarViewModel { get; }
        public GrmNodeRequests GrmNodeRequests { get; }
        public GrmEquipmentRequests GrmEquipmentRequests { get; }
        public GrmFiberRequests GrmFiberRequests { get; }
        public GrmFiberWithNodesRequest GrmFiberWithNodesRequest { get; }
        public GrmRtuRequests GrmRtuRequests { get; }
        public IWindowManager WindowManager { get; }
        public ReadModel ReadModel { get; }
        public readonly ILifetimeScope GlobalScope;
        public readonly IniFile IniFile;

        public ObservableCollection<NodeVm> Nodes { get; }
        public ObservableCollection<FiberVm> Fibers { get; }
        public ObservableCollection<RtuVm> Rtus { get; }
        public ObservableCollection<EquipmentVm> Equipments { get; }
        public ObservableCollection<TraceVm> Traces { get; }

      
        public List<GraphVisibilityLevelItem> GraphVisibilityItems { get; set; } 
        private GraphVisibilityLevelItem _selectedGraphVisibilityItem;

        public GraphVisibilityLevelItem SelectedGraphVisibilityItem
        {
            get => _selectedGraphVisibilityItem;
            set
            {
                if (value == _selectedGraphVisibilityItem) return;
                _selectedGraphVisibilityItem = value;
                IniFile.Write(IniSection.Miscellaneous, IniKey.GraphVisibilityLevel, SelectedGraphVisibilityItem.Level.ToString());
                NotifyOfPropertyChange();
            }
        }

        public GraphReadModel(ILifetimeScope globalScope, IniFile iniFile, CurrentGpsInputMode currentGpsInputMode,  
            CommonStatusBarViewModel commonStatusBarViewModel,
            GrmNodeRequests grmNodeRequests, GrmEquipmentRequests grmEquipmentRequests, 
            GrmFiberRequests grmFiberRequests, GrmFiberWithNodesRequest grmFiberWithNodesRequest,
             GrmRtuRequests grmRtuRequests,
            
            IWindowManager windowManager, ReadModel readModel)
        {
            CurrentGpsInputMode = currentGpsInputMode;
            CommonStatusBarViewModel = commonStatusBarViewModel;
            GrmNodeRequests = grmNodeRequests;
            GrmEquipmentRequests = grmEquipmentRequests;
            GrmFiberRequests = grmFiberRequests;
            GrmFiberWithNodesRequest = grmFiberWithNodesRequest;
            GrmRtuRequests = grmRtuRequests;
            WindowManager = windowManager;
            ReadModel = readModel;
            GlobalScope = globalScope;
            IniFile = iniFile;
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

          //  MainMap.Initialize(currentGpsInputMode);
        }

        public void SetGraphVisibility(GraphVisibilityLevel level)
        {
            SelectedGraphVisibilityItem =
                GraphVisibilityItems.First(i => i.Level == level);
            IniFile.Write(IniSection.Miscellaneous, IniKey.GraphVisibilityLevel, level.ToString());
        }

        public void PlaceRtuIntoScreenCenter(Guid rtuId)
        {
            var nodeVm = Rtus.First(r => r.Id == rtuId).Node;
            nodeVm.IsHighlighted = true;
            MainMap.Position = nodeVm.Position;
        }

        public void ExtinguishNode()
        {
            var nodeVm = Nodes.FirstOrDefault(n => n.IsHighlighted);
            if (nodeVm != null)
                nodeVm.IsHighlighted = false;
        }

        public void ChangeTraceColor(Guid traceId, List<Guid> nodes, FiberState state)
        {
            var fibers = this.GetFibersByNodes(nodes);
            foreach (var fiberVm in fibers)
            {
                if (state != FiberState.NotInTrace)
                    fiberVm.SetState(traceId, state);
                else
                    fiberVm.RemoveState(traceId);
            }
        }

    }
}