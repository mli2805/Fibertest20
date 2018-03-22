using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class TraceLeafActions
    {
        private readonly ILifetimeScope _globalScope;
        private readonly ReadModel _readModel;
        private readonly IWindowManager _windowManager;
        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly TraceStateViewsManager _traceStateViewsManager;
        private readonly TraceStatisticsViewsManager _traceStatisticsViewsManager;
        private readonly BaseRefsAssignViewModel _baseRefsAssignViewModel;
        private readonly LandmarksViewModel _landmarksViewModel;
        private readonly OutOfTurnPreciseMeasurementViewModel _outOfTurnPreciseMeasurementViewModel;
        private readonly CommonStatusBarViewModel _commonStatusBarViewModel;

        public TraceLeafActions(ILifetimeScope globalScope, ReadModel readModel,
            IWindowManager windowManager, IWcfServiceForClient c2DWcfManager,
            TraceStateViewsManager traceStateViewsManager, TraceStatisticsViewsManager traceStatisticsViewsManager,
            BaseRefsAssignViewModel baseRefsAssignViewModel, LandmarksViewModel landmarksViewModel,
            OutOfTurnPreciseMeasurementViewModel outOfTurnPreciseMeasurementViewModel,
            CommonStatusBarViewModel commonStatusBarViewModel)
        {
            _globalScope = globalScope;
            _readModel = readModel;
            _windowManager = windowManager;
            _c2DWcfManager = c2DWcfManager;
            _traceStateViewsManager = traceStateViewsManager;
            _traceStatisticsViewsManager = traceStatisticsViewsManager;
            _baseRefsAssignViewModel = baseRefsAssignViewModel;
            _landmarksViewModel = landmarksViewModel;
            _outOfTurnPreciseMeasurementViewModel = outOfTurnPreciseMeasurementViewModel;
            _commonStatusBarViewModel = commonStatusBarViewModel;
        }

        public void UpdateTrace(object param)
        {
            if (!(param is TraceLeaf traceLeaf))
                return;
            var trace = _readModel.Traces.FirstOrDefault(t => t.Id == traceLeaf.Id);
            if (trace == null)
                return;
            var vm = _globalScope.Resolve<TraceInfoViewModel>();
            vm.Initialize(traceLeaf.Id, trace.Equipments, trace.Nodes);
            _windowManager.ShowWindowWithAssignedOwner(vm);
        }

        public void ShowTrace(object param)
        {
        }

        public void AssignBaseRefs(object param)
        {
            if (!(param is TraceLeaf traceLeaf))
                return;

            var trace = _readModel.Traces.First(t => t.Id == traceLeaf.Id);
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

        public void ShowTraceLandmarks(object param)
        {
            if (!(param is TraceLeaf traceLeaf))
                return;

            _landmarksViewModel.Initialize(traceLeaf.Id, false);
            _windowManager.ShowWindowWithAssignedOwner(_landmarksViewModel);
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
                var cmd = isRemoval ? new RemoveTrace() { Id = traceId } : (object)new CleanTrace() { Id = traceId };
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