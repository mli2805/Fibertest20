using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using Caliburn.Micro;
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
        private readonly RenderingApplierToUi _renderingApplierToUi;
        private readonly CurrentlyHiddenRtu _currentlyHiddenRtu;
        private readonly CurrentUser _currentUser;
        private readonly GraphReadModel _graphReadModel;
        private readonly WaitViewModel _waitViewModel;

        public RenderingManager(IMyLog logFile, IWindowManager windowManager, IDispatcherProvider dispatcherProvider,
            CurrentZoneRenderer currentZoneRenderer,
             RenderingApplierToUi renderingApplierToUi, CurrentlyHiddenRtu currentlyHiddenRtu,
            CurrentUser currentUser, GraphReadModel graphReadModel,
            WaitViewModel waitViewModel)
        {
            _logFile = logFile;
            _windowManager = windowManager;
            _dispatcherProvider = dispatcherProvider;
            _currentZoneRenderer = currentZoneRenderer;
            _renderingApplierToUi = renderingApplierToUi;
            _currentlyHiddenRtu = currentlyHiddenRtu;
            _currentUser = currentUser;
            _graphReadModel = graphReadModel;
            _waitViewModel = waitViewModel;
        }

        public void Initialize()
        {
            _currentlyHiddenRtu.Initialize();
            _currentlyHiddenRtu.PropertyChanged += _currentlyHiddenRtu_PropertyChanged;
        }

        private async void _currentlyHiddenRtu_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == @"IsShowAllPressed" || e.PropertyName == @"IsHideAllPressed")
            {
                var unused = await RenderOnAllShowOrHidePressed();
            }

            if (e.PropertyName == @"ChangedRtu")
            {
                await RenderOnRtuChanged();
            }
            _currentlyHiddenRtu.CleanFlags();
        }

        private async Task<int> RenderOnAllShowOrHidePressed()
        {
            _waitViewModel.Initialize(LongOperation.DrawingGraph);
            _windowManager.ShowWindowWithAssignedOwner(_waitViewModel);

            var unused1 = await FullClean();
            var renderingResult = _currentlyHiddenRtu.Collection.Count == 0
                ? await Task.Factory.StartNew(_currentZoneRenderer.GetRenderingForShowAll)
                : await Task.Factory.StartNew(_currentZoneRenderer.GetRenderingForHiddenAll);

            var unused = await _renderingApplierToUi.ToEmptyGraph(renderingResult);
            _currentlyHiddenRtu.CleanFlags();
            _waitViewModel.TryClose();

            return unused;
        }

        public async Task<int> RenderOnRtuChanged()
        {
            _waitViewModel.Initialize(LongOperation.DrawingGraph);
            _windowManager.ShowWindowWithAssignedOwner(_waitViewModel);

            var unused1 = await FullClean();
            var renderingResult = await Task.Factory.StartNew(_currentZoneRenderer.GetCurrentRendering);

            var unused = await _renderingApplierToUi.ToEmptyGraph(renderingResult);
            _currentlyHiddenRtu.CleanFlags();
            _waitViewModel.TryClose();

            return unused;
        }

        public async Task RenderCurrentZoneOnApplicationStart()
        {
            _waitViewModel.Initialize(LongOperation.DrawingGraph);
            _windowManager.ShowWindowWithAssignedOwner(_waitViewModel);

            _logFile.AppendLine(@"rendering started");
            var renderingResult = _currentlyHiddenRtu.Collection.Count == 0
                ? await Task.Factory.StartNew(_currentZoneRenderer.GetRenderingForShowAll)
                : await Task.Factory.StartNew(_currentZoneRenderer.GetRenderingForHiddenAll);
            var unused = await _renderingApplierToUi.ToEmptyGraph(renderingResult);

            // InvokeAsync hangs up all tests
            _dispatcherProvider.GetDispatcher().Invoke(() => _waitViewModel.TryClose(), DispatcherPriority.ApplicationIdle);
            _logFile.AppendLine(@"rendering finished");
        }

        public async void ReRenderCurrentZoneOnResponsibilitiesChanged()
        {
            if (_currentUser.ZoneId == Guid.Empty) return; // it is a default zone user

            _waitViewModel.Initialize(LongOperation.DrawingGraph);
            _windowManager.ShowWindowWithAssignedOwner(_waitViewModel);

            var unused1 = await FullClean();
            var renderingResult = await Task.Factory.StartNew(() => _currentZoneRenderer.GetCurrentRendering());
            var unused = await _renderingApplierToUi.ToEmptyGraph(renderingResult);

            _waitViewModel.TryClose();
        }

        private async Task<int> FullClean()
        {
            await Task.Delay(2);
            _graphReadModel.Data.Fibers.Clear();
            await Task.Delay(2);
            _graphReadModel.Data.Nodes.Clear();
            await Task.Delay(2);
            if (_graphReadModel.MainMap == null) return 0; // under tests
            for (int i = _graphReadModel.MainMap.Markers.Count - 1; i >= 0; i--)
            {
                _graphReadModel.MainMap.Markers.RemoveAt(i);
                if (i % 100 == 0) await Task.Delay(2);
            }

            return 1;
        }
    }
}