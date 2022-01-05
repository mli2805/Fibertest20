using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
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

        private bool _isInGisVisibleMode = true;
        public bool IsInGisVisibleMode
        {
            get { return _isInGisVisibleMode; }
            set
            {
                if (value == _isInGisVisibleMode) return;
                _isInGisVisibleMode = value;
                NotifyOfPropertyChange();
            }
        }

        public IMyLog LogFile { get; }
        public CurrentGis CurrentGis { get; }
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

        public GraphReadModelData Data { get; set; } = new GraphReadModelData();

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
            CurrentGis currentGis, CurrentUser currentUser,
            CommonStatusBarViewModel commonStatusBarViewModel,
            GrmNodeRequests grmNodeRequests, GrmEquipmentRequests grmEquipmentRequests,
            GrmFiberRequests grmFiberRequests, GrmFiberWithNodesRequest grmFiberWithNodesRequest,
             GrmRtuRequests grmRtuRequests,
            IWindowManager windowManager, Model readModel)
        {
            LogFile = logFile;
            CurrentGis = currentGis;
            currentGis.PropertyChanged += CurrentGis_PropertyChanged;
            currentGis.Traces.CollectionChanged += Traces_CollectionChanged;
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

        private async void Traces_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            await RefreshVisiblePart();
        }

        private void CurrentGis_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            IsInGisVisibleMode = ((CurrentGis)sender).IsGisOn;
        }

        public async Task<int> RefreshVisiblePart()
        {
            var renderingResult = await this.RenderForRoot();

            var nodeVmCount = await this.ToExistingGraph(renderingResult);

            if (MainMap != null)
                MainMap.NodeCountString = $@" {ReadModel.Nodes.Count} / {nodeVmCount}";

            return nodeVmCount;
        }

        public void SetGraphVisibility(GraphVisibilityLevel level)
        {
            SelectedGraphVisibilityItem =
                GraphVisibilityItems.First(i => i.Level == level);
            IniFile.Write(IniSection.Miscellaneous, IniKey.GraphVisibilityLevel, level.ToString());
        }

        public void PlacePointIntoScreenCenter(PointLatLng position)
        {
            MainMap.SetPosition(position);
        }

        public void ShowTrace(Trace trace)
        {
            // RTU into screen center
            var node = ReadModel.Nodes.First(n => n.NodeId == trace.NodeIds[0]);
            MainMap.SetPosition(node.Position);

            HighlightTrace(trace);
        }

        public void NodeToCenterAndHighlight(Guid nodeId)
        {
            var node = ReadModel.Nodes.First(n => n.NodeId == nodeId);
            MainMap.SetPosition(node.Position);

            node.IsHighlighted = true;
            var nodeVm = Data.Nodes.FirstOrDefault(n => n.Id == nodeId);
            if (nodeVm != null)
                nodeVm.IsHighlighted = true;
        }

        public void HighlightTrace(Trace trace)
        {
            SetTraceLight(trace, true);
        }

        public void ExtinguishTrace(Trace trace)
        {
            SetTraceLight(trace, false);
        }

        private void SetTraceLight(Trace trace, bool highlight)
        {
            foreach (var fiberId in trace.FiberIds)
            {
                ReadModel.Fibers.First(f => f.FiberId == fiberId).SetLightOnOff(trace.TraceId, highlight);

                var fiberVm = Data.Fibers.FirstOrDefault(f => f.Id == fiberId);
                if (fiberVm != null)
                    fiberVm.SetLightOnOff(trace.TraceId, highlight);
            }
        }

        public void ExtinguishAll()
        {
            CurrentGis.Traces.Clear();

            foreach (var nodeVm in Data.Nodes.Where(n => n.IsHighlighted))
                nodeVm.IsHighlighted = false;

            foreach (var fiberVm in Data.Fibers.Where(f => f.HighLights.Any()))
                fiberVm.ClearLight();

            foreach (var fiber in ReadModel.Fibers)
            {
                fiber.HighLights = new List<Guid>();
            }
        }

        public void ExtinguishNodes()
        {
            foreach (var nodeVm in Data.Nodes.Where(n => n.IsHighlighted))
                nodeVm.IsHighlighted = false;
        }

        public void ChangeTraceColor(Guid traceId, FiberState state)
        {
            var trace = ReadModel.Traces.FirstOrDefault(t => t.TraceId == traceId);
            if (trace == null) return;

            var fibers = ReadModel.GetTraceFibers(trace);
            foreach (var fiber in fibers)
            {
                var fiberVm = Data.Fibers.FirstOrDefault(f => f.Id == fiber.FiberId);
                if (fiberVm == null) continue;
                if (state != FiberState.NotInTrace)
                    fiberVm.SetState(traceId, state);
                else
                    fiberVm.RemoveState(traceId);
            }
        }

        public void SetFutureTraceLightOnOff(Guid traceId, List<Guid> fiberIds, bool light)
        {
            foreach (var fiberId in fiberIds)
            {
                var fiber = ReadModel.Fibers.First(f => f.FiberId == fiberId);
                fiber.SetLightOnOff(traceId, light);

                var fiberVm = Data.Fibers.FirstOrDefault(f => f.Id == fiberId);
                fiberVm?.SetLightOnOff(traceId, light);
            }
        }

        public void ChangeFutureTraceColor(Guid traceId, List<Guid> fiberIds, FiberState state)
        {
            foreach (var fiberId in fiberIds)
            {
                var fiberVm = Data.Fibers.FirstOrDefault(f => f.Id == fiberId);
                if (fiberVm != null)
                {
                    if (state != FiberState.NotInTrace)
                        fiberVm.SetState(traceId, state);
                    else
                        fiberVm.RemoveState(traceId);
                }
            }
        }
    }
}