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

        public void ShowTrace(Guid rtuNodeId, List<Guid> fibers)
        {
            var nodeVm = Data.Nodes.First(n => n.Id == rtuNodeId);
            MainMap.Position = nodeVm.Position;
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
        public void ExtinguishNode()
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

        public void ChangeFutureTraceColor(Guid traceId, List<Guid> fiberIds, FiberState state)
        {

            foreach (var fiberId in fiberIds)
            {
                var fiberVm = Data.Fibers.First(f => f.Id == fiberId);
                if (state != FiberState.NotInTrace)
                    fiberVm.SetState(traceId, state);
                else
                    fiberVm.RemoveState(traceId);
            }
        }

    }
}