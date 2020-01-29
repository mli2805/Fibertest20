using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WpfCommonViews;


namespace Iit.Fibertest.Client
{
    public class TraceLeafActions
    {
        private readonly ILifetimeScope _globalScope;
        private readonly Model _readModel;
        private readonly GraphReadModel _graphReadModel;
        private readonly IWindowManager _windowManager;
        private readonly IWcfServiceDesktopC2D _c2DWcfManager;
        private readonly TabulatorViewModel _tabulatorViewModel;
        private readonly TraceStateViewsManager _traceStateViewsManager;
        private readonly TraceStatisticsViewsManager _traceStatisticsViewsManager;
        private readonly BaseRefsAssignViewModel _baseRefsAssignViewModel;
        private readonly LandmarksViewsManager _landmarksViewsManager;
        private readonly OutOfTurnPreciseMeasurementViewModel _outOfTurnPreciseMeasurementViewModel;
        private readonly CommonStatusBarViewModel _commonStatusBarViewModel;
        private readonly CurrentlyHiddenRtu _currentlyHiddenRtu;
        private readonly RenderingManager _renderingManager;

        public TraceLeafActions(ILifetimeScope globalScope, Model readModel, GraphReadModel graphReadModel,
            IWindowManager windowManager, IWcfServiceDesktopC2D c2DWcfManager, TabulatorViewModel tabulatorViewModel,
            TraceStateViewsManager traceStateViewsManager, TraceStatisticsViewsManager traceStatisticsViewsManager,
            BaseRefsAssignViewModel baseRefsAssignViewModel, LandmarksViewsManager landmarksViewsManager,
            OutOfTurnPreciseMeasurementViewModel outOfTurnPreciseMeasurementViewModel,
            CommonStatusBarViewModel commonStatusBarViewModel,
            CurrentlyHiddenRtu currentlyHiddenRtu, RenderingManager renderingManager)
        {
            _globalScope = globalScope;
            _readModel = readModel;
            _graphReadModel = graphReadModel;
            _windowManager = windowManager;
            _c2DWcfManager = c2DWcfManager;
            _tabulatorViewModel = tabulatorViewModel;
            _traceStateViewsManager = traceStateViewsManager;
            _traceStatisticsViewsManager = traceStatisticsViewsManager;
            _baseRefsAssignViewModel = baseRefsAssignViewModel;
            _landmarksViewsManager = landmarksViewsManager;
            _outOfTurnPreciseMeasurementViewModel = outOfTurnPreciseMeasurementViewModel;
            _commonStatusBarViewModel = commonStatusBarViewModel;
            _currentlyHiddenRtu = currentlyHiddenRtu;
            _renderingManager = renderingManager;
        }

        public void UpdateTrace(object param)
        {
            if (!(param is TraceLeaf traceLeaf))
                return;
            var trace = _readModel.Traces.FirstOrDefault(t => t.TraceId == traceLeaf.Id);
            if (trace == null)
                return;
            var vm = _globalScope.Resolve<TraceInfoViewModel>();
            vm.Initialize(traceLeaf.Id, trace.EquipmentIds, trace.NodeIds, false);
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public async void HighlightTrace(object param)
        {
            if (!(param is TraceLeaf traceLeaf))
                return;
            var trace = _readModel.Traces.First(t => t.TraceId == traceLeaf.Id);

            if (_currentlyHiddenRtu.Collection.Contains(trace.RtuId))
            {
                _currentlyHiddenRtu.Collection.Remove(trace.RtuId);
                var unused = await _renderingManager.RenderOnRtuChanged();
            }
            _graphReadModel.HighlightTrace(trace.NodeIds[0], trace.FiberIds);
            trace.IsHighlighted = true;

            if (_tabulatorViewModel.SelectedTabIndex != 3)
                _tabulatorViewModel.SelectedTabIndex = 3;
        }

        public void AssignBaseRefs(object param)
        {
            if (!(param is TraceLeaf traceLeaf))
                return;

            var trace = _readModel.Traces.First(t => t.TraceId == traceLeaf.Id);
            _baseRefsAssignViewModel.Initialize(trace);
            _windowManager.ShowDialogWithAssignedOwner(_baseRefsAssignViewModel);
        }

        public void ShowTraceState(object param)
        {
            if (!(param is TraceLeaf traceLeaf))
                return;

            _traceStateViewsManager.ShowTraceState(traceLeaf.Id);
        }

        public void ShowTraceStatistics(object param)
        {
            if (!(param is TraceLeaf traceLeaf))
                return;

            _traceStatisticsViewsManager.Show(traceLeaf.Id);
        }

        public async void ShowTraceLandmarks(object param)
        {
            if (!(param is TraceLeaf traceLeaf))
                return;
            var rtuNodeId = _readModel.Traces.First(t => t.TraceId == traceLeaf.Id).NodeIds[0];
            await _landmarksViewsManager.InitializeFromTrace(traceLeaf.Id, rtuNodeId);
        }

        public async void DetachTrace(object param)
        {
            if (!(param is TraceLeaf traceLeaf))
                return;

            await _c2DWcfManager.SendCommandAsObj(new DetachTrace() { TraceId = traceLeaf.Id });
        }

        public async void CleanTrace(object param)
        {
            await DoCleanOrRemoveTrace(param, false);
        }

        public async void RemoveTrace(object param)
        {
            await DoCleanOrRemoveTrace(param, true);
        }

        private async Task DoCleanOrRemoveTrace(object param, bool isRemoval)
        {
            if (!(param is TraceLeaf traceLeaf))
                return;
            var traceId = traceLeaf.Id;

            var question = AssembleTraceRemovalConfirmation(traceLeaf.Title);
            var vm = new MyMessageBoxViewModel(MessageType.Confirmation, question);
            _windowManager.ShowDialogWithAssignedOwner(vm);

            if (vm.IsAnswerPositive)
            {
                _commonStatusBarViewModel.StatusBarMessage2 = Resources.SID_Long_operation__Removing_trace_s_measurements____Please_wait_;
                var cmd = isRemoval ? new RemoveTrace() { TraceId = traceId } : (object)new CleanTrace() { TraceId = traceId };
                var result = await _c2DWcfManager.SendCommandAsObj(cmd);
                _commonStatusBarViewModel.StatusBarMessage2 = result ?? "";
            }
        }

        private static List<MyMessageBoxLineModel> AssembleTraceRemovalConfirmation(string traceTitle)
        {
            var list = new List<MyMessageBoxLineModel>
            {
                new MyMessageBoxLineModel() {Line = Resources.SID_Attention_},
                new MyMessageBoxLineModel() {Line = Resources.SID_All_measurements_for_trace},
                new MyMessageBoxLineModel() {Line = ""},
                new MyMessageBoxLineModel() {Line = $@"{traceTitle}", FontWeight = FontWeights.Bold},
                new MyMessageBoxLineModel() {Line = ""},
                new MyMessageBoxLineModel() {Line = Resources.SID_will_be_removed},
                new MyMessageBoxLineModel() {Line = ""},
                new MyMessageBoxLineModel() {Line = Resources.SID_Are_you_sure_},
            };
            return list;
        }

        public void DoPreciseMeasurementOutOfTurn(object param)
        {
            if (!(param is TraceLeaf traceLeaf))
                return;

            _outOfTurnPreciseMeasurementViewModel.Initialize(traceLeaf);
            _windowManager.ShowDialogWithAssignedOwner(_outOfTurnPreciseMeasurementViewModel);
        }
    }
}