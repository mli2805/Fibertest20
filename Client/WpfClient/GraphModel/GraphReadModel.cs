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
        public Map MainMap { get; set; }
        public bool IsInGisVisibleMode { get; set; }
        public IMyLog LogFile { get; }
        public CurrentGpsInputMode CurrentGpsInputMode { get; }
        public CurrentUser CurrentUser { get; }
        public CommonStatusBarViewModel CommonStatusBarViewModel { get; }
        public GrmNodeRequests GrmNodeRequests { get; }
        public GrmEquipmentRequests GrmEquipmentRequests { get; }
        public GrmFiberRequests GrmFiberRequests { get; }
        public GrmFiberWithNodesRequest GrmFiberWithNodesRequest { get; }
        public GrmRtuRequests GrmRtuRequests { get; }
        public IWindowManager WindowManager { get; }
        public Model ReadModel { get; }
        public readonly ILifetimeScope GlobalScope;
        public readonly IniFile IniFile;

        public GrmData Data { get; set; } = new GrmData();

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

        public GraphReadModel(ILifetimeScope globalScope, IniFile iniFile, IMyLog logFile, 
            CurrentGpsInputMode currentGpsInputMode, CurrentUser currentUser, 
            CurrentDatacenterParameters currentDatacenterParameters,
            CommonStatusBarViewModel commonStatusBarViewModel,
            GrmNodeRequests grmNodeRequests, GrmEquipmentRequests grmEquipmentRequests,
            GrmFiberRequests grmFiberRequests, GrmFiberWithNodesRequest grmFiberWithNodesRequest,
             GrmRtuRequests grmRtuRequests,
            IWindowManager windowManager, Model readModel)
        {
            LogFile = logFile;
            CurrentGpsInputMode = currentGpsInputMode;
            CurrentUser = currentUser;
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
            IsInGisVisibleMode = currentDatacenterParameters.IsInGisVisibleMode;
            Data.Nodes = new ObservableCollection<NodeVm>();
            Data.Fibers = new ObservableCollection<FiberVm>();

            GraphVisibilityItems = GraphVisibilityExt.GetComboboxItems();
            var levelString = iniFile.Read(IniSection.Miscellaneous, IniKey.GraphVisibilityLevel,
                GraphVisibilityLevel.AllDetails.ToString());
            if (!Enum.TryParse(levelString, out GraphVisibilityLevel level))
                level = GraphVisibilityLevel.AllDetails;
            SetGraphVisibility(level);
        }

        public void SetGraphVisibility(GraphVisibilityLevel level)
        {
            SelectedGraphVisibilityItem =
                GraphVisibilityItems.First(i => i.Level == level);
            IniFile.Write(IniSection.Miscellaneous, IniKey.GraphVisibilityLevel, level.ToString());
        }

        public void PlaceRtuIntoScreenCenter(Guid rtuId)
        {
            var rtu = ReadModel.Rtus.First(r => r.Id == rtuId);

            var nodeVm = Data.Nodes.First(n => n.Id == rtu.NodeId);
            nodeVm.IsHighlighted = true;
            MainMap.Position = nodeVm.Position;
        }

        public void PlaceNodeIntoScreenCenter(Guid nodeId)
        {
            var nodeVm = Data.Nodes.First(n => n.Id == nodeId);
            nodeVm.IsHighlighted = true;
            MainMap.Position = nodeVm.Position;
        }

        public void PlacePointIntoScreenCenter(PointLatLng position)
        {
            MainMap.Position = position;
        }

        public void HighlightTrace(Guid rtuNodeId, List<Guid> fibers)
        {
            var rtuNodeVm = Data.Nodes.First(n => n.Id == rtuNodeId);
            MainMap.Position = rtuNodeVm.Position;
            foreach (var fiberId in fibers)
                Data.Fibers.First(f => f.Id == fiberId).IsHighlighted = true; 
        }
      
        public void Extinguish()
        {
            foreach (var nodeVm in Data.Nodes.Where(n => n.IsHighlighted))
                nodeVm.IsHighlighted = false;

            foreach (var fiberVm in Data.Fibers.Where(f=>f.IsHighlighted))
                fiberVm.IsHighlighted = false;
        }
        public void ExtinguishNodes()
        {
            foreach (var nodeVm in Data.Nodes.Where(n=>n.IsHighlighted))
                nodeVm.IsHighlighted = false;
        }

        public void ChangeTraceColor(Guid traceId, FiberState state)
        {
            var trace = ReadModel.Traces.FirstOrDefault(t => t.TraceId == traceId);
            if (trace == null) return;

            var fibers = ReadModel.GetTraceFibers(trace);
            foreach (var fiber in fibers)
            {
                var fiberVm = Data.Fibers.First(f => f.Id == fiber.FiberId);
                if (state != FiberState.NotInTrace)
                    fiberVm.SetState(traceId, state);
                else
                    fiberVm.RemoveState(traceId);
            }
        }

        public bool ChangeFutureTraceColor(Guid traceId, List<Guid> fiberIds, FiberState state)
        {
            foreach (var fiberId in fiberIds)
            {
                var fiberVm = Data.Fibers.FirstOrDefault(f => f.Id == fiberId);
                if (fiberVm == null)
                {
                    LogFile.AppendLine($@"Fiber {fiberId.First6()} was not found");
                    return false;
                }
                if (state != FiberState.NotInTrace)
                    fiberVm.SetState(traceId, state);
                else
                    fiberVm.RemoveState(traceId);
            }
            return true;
        }
    }
}