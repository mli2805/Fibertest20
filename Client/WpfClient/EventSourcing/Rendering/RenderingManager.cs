using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WpfCommonViews;
using Trace = Iit.Fibertest.Graph.Trace;


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
        private readonly WaitViewModel _waitViewModel;

        public RenderingManager(IMyLog logFile, IWindowManager windowManager, IDispatcherProvider dispatcherProvider,
            CurrentZoneRenderer currentZoneRenderer, OneRtuOrTraceRenderer oneRtuOrTraceRenderer,
             RenderingApplier renderingApplier, CurrentlyHiddenRtu currentlyHiddenRtu,
            CurrentUser currentUser, GraphReadModel graphReadModel,
            RootRenderer rootRenderer, LessThanRootRenderer lessThanRootRenderer, WaitViewModel waitViewModel)
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
            _waitViewModel = waitViewModel;
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
                //  var vm = MyMessageBoxExt.DrawingGraph();
                _windowManager.ShowWindowWithAssignedOwner(_waitViewModel);

                var renderingResult = await Task.Factory.StartNew(() => _currentZoneRenderer.GetRendering());
                //                var  renderingResult = await _dispatcherProvider.GetDispatcher().Invoke(() => _currentZoneRenderer.GetRenderingAsync(), DispatcherPriority.ApplicationIdle);

                _renderingApplier.ToExistingGraphUi(renderingResult);

                // InvokeAsync hangs up all tests
                //                _dispatcherProvider.GetDispatcher().Invoke(() => vm.TryClose(), DispatcherPriority.ApplicationIdle);
                _waitViewModel.TryClose();
            }

            _currentlyHiddenRtu.CleanFlags();
        }

        public async Task RenderCurrentZoneOnApplicationStart()
        {
            //            var vm = MyMessageBoxExt.DrawingGraph();
            _windowManager.ShowWindowWithAssignedOwner(_waitViewModel);

            // await ShowOrHideAllOnEmptyMap();
          
            var renderingResult = await Task.Factory.StartNew(RenderAll);
            _renderingApplier.ToEmptyGraph(renderingResult);


            // InvokeAsync hangs up all tests
            _dispatcherProvider.GetDispatcher().Invoke(() => _waitViewModel.TryClose(), DispatcherPriority.ApplicationIdle);
        }


        private async Task<int> ShowOrHideAllOnEmptyMap()
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

            return 1;
        }

        private RenderingResult RenderAll()
        {
            var renderingResult = (_currentUser.Role <= Role.Root)
            ?
                 _currentlyHiddenRtu.Collection.Count == 0
                    ? _rootRenderer.ShowAll()
                    : _rootRenderer.ShowOnlyRtusAndNotInTraces()
            :
                 _currentlyHiddenRtu.Collection.Count == 0
                    ? _lessThanRootRenderer.ShowAllOnStart()
                    : _lessThanRootRenderer.ShowOnlyRtus();

            return renderingResult;
        }

        public async void ReRenderCurrentZoneOnResponsibilitiesChanged()
        {
            if (_currentUser.ZoneId == Guid.Empty) return; // it is a default zone user

            //            var vm = MyMessageBoxExt.DrawingGraph();
            _windowManager.ShowWindowWithAssignedOwner(_waitViewModel);

            var renderingResult = await Task.Factory.StartNew(() => _currentZoneRenderer.GetRendering());
            //  var renderingResult = await _currentZoneRenderer.GetRenderingAsync();
            _renderingApplier.ToExistingGraphUi(renderingResult);

            _waitViewModel.TryClose();
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