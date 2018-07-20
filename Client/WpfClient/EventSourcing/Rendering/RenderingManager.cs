using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WpfCommonViews;


namespace Iit.Fibertest.Client
{
    public class RenderingManager
    {
        private readonly IMyLog _logFile;
        private readonly IWindowManager _windowManager;
        private readonly IDispatcherProvider _dispatcherProvider;
        private readonly CurrentZoneRenderer _currentZoneRenderer;
        private readonly OneRtuOrTraceRenderer _oneRtuOrTraceRenderer;
        private readonly RenderingApplier _renderingApplier;
        private readonly CurrentlyHiddenRtu _currentlyHiddenRtu;
        private readonly CurrentUser _currentUser;
        private readonly GraphReadModel _graphReadModel;
        private readonly RootRenderer _rootRenderer;
        private readonly LessThanRootRenderer _lessThanRootRenderer;

        public RenderingManager(IMyLog logFile, IWindowManager windowManager, IDispatcherProvider dispatcherProvider,
            CurrentZoneRenderer currentZoneRenderer, OneRtuOrTraceRenderer oneRtuOrTraceRenderer,
             RenderingApplier renderingApplier, CurrentlyHiddenRtu currentlyHiddenRtu,
            CurrentUser currentUser, GraphReadModel graphReadModel,
            RootRenderer rootRenderer, LessThanRootRenderer lessThanRootRenderer)
        {
            _logFile = logFile;
            _windowManager = windowManager;
            _dispatcherProvider = dispatcherProvider;
            _currentZoneRenderer = currentZoneRenderer;
            _oneRtuOrTraceRenderer = oneRtuOrTraceRenderer;
            _renderingApplier = renderingApplier;
            _currentlyHiddenRtu = currentlyHiddenRtu;
            _currentUser = currentUser;
            _graphReadModel = graphReadModel;
            _rootRenderer = rootRenderer;
            _lessThanRootRenderer = lessThanRootRenderer;
        }

        public void Initialize()
        {
            _currentlyHiddenRtu.Initialize();
            _currentlyHiddenRtu.PropertyChanged += _currentlyHiddenRtu_PropertyChanged;
        }

        private async void _currentlyHiddenRtu_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsShowAllPressed" || e.PropertyName == "IsHideAllPressed")
            {
                FullClean();
                await ShowOrHideAllOnEmptyMap();
            }

            if (e.PropertyName == "ChangedRtu")
            {
                var renderingResult = _currentZoneRenderer.GetRendering();
                _renderingApplier.ToExistingGraph(renderingResult);
             }

            _currentlyHiddenRtu.CleanFlags();
        }

        public async Task RenderCurrentZoneOnApplicationStart()
        {
            var vm = MyMessageBoxExt.DrawingGraph();
            _windowManager.ShowWindowWithAssignedOwner(vm);
            await ShowOrHideAllOnEmptyMap();
            // InvokeAsync hangs up all tests
            _dispatcherProvider.GetDispatcher().Invoke(() => vm.TryClose(), DispatcherPriority.ApplicationIdle);
        }


        private async Task ShowOrHideAllOnEmptyMap()
        {
            await Task.Delay(1); // just to get rid of warning
            if (_currentUser.Role <= Role.Root)
            {
                var renderingResult = _currentlyHiddenRtu.Collection.Count == 0
                    ? _rootRenderer.ShowAll()
                    : _rootRenderer.ShowOnlyRtusAndNotInTraces();
                _renderingApplier.ToEmptyGraph(renderingResult);
            }
            else
            {
                var renderingResult = _currentlyHiddenRtu.Collection.Count == 0
                    ? _lessThanRootRenderer.ShowAllOnStart()
                    : _lessThanRootRenderer.ShowOnlyRtus();
                _renderingApplier.ToEmptyGraph(renderingResult);
            }
        }

        public void ReRenderCurrentZoneOnResponsibilitiesChanged()
        {
            if (_currentUser.ZoneId == Guid.Empty) return; // it is a default zone user

            var renderingResult = _currentZoneRenderer.GetRendering();
            _renderingApplier.ToExistingGraph(renderingResult);
        }

        // User asks to show broken or highlight trace
        public void ShowOneTrace(Trace trace)
        {
            var renderingResult = new RenderingResult();
            _oneRtuOrTraceRenderer.GetTraceRendering(trace, renderingResult);
            _renderingApplier.AddElementsOfShownTraces(renderingResult);
        }

        private void FullClean()
        {
            _graphReadModel.Data.Fibers.Clear();
            _graphReadModel.Data.Nodes.Clear();
            var start = DateTime.Now;
            for (int i = _graphReadModel.MainMap.Markers.Count - 1; i >= 0; i--)
            {
                _graphReadModel.MainMap.Markers.RemoveAt(i);
            }
            _logFile.AppendLine($@"MainMap.Markers are cleaned in {DateTime.Now - start}");
        }
    }
}