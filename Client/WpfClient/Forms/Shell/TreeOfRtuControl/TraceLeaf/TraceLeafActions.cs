using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class TraceLeafActions
    {
        private readonly ReadModel _readModel;
        private readonly IWindowManager _windowManager;
        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly TraceStateViewsManager _traceStateViewsManager;
        private readonly TraceStatisticsViewsManager _traceStatisticsViewsManager;
        private readonly ReflectogramManager _reflectogramManager;
        private readonly BaseRefsAssignViewModel _baseRefsAssignViewModel;
        private readonly OpticalEventsDoubleViewModel _opticalEventsDoubleViewModel;
        private readonly CommonStatusBarViewModel _commonStatusBarViewModel;

        public TraceLeafActions(ReadModel readModel,
            IWindowManager windowManager, IWcfServiceForClient c2DWcfManager,
            TraceStateViewsManager traceStateViewsManager, TraceStatisticsViewsManager traceStatisticsViewsManager,
             ReflectogramManager reflectogramManager, BaseRefsAssignViewModel baseRefsAssignViewModel,
            OpticalEventsDoubleViewModel opticalEventsDoubleViewModel,
            CommonStatusBarViewModel commonStatusBarViewModel)
        {
            _readModel = readModel;
            _windowManager = windowManager;
            _c2DWcfManager = c2DWcfManager;
            _traceStateViewsManager = traceStateViewsManager;
            _traceStatisticsViewsManager = traceStatisticsViewsManager;
            _reflectogramManager = reflectogramManager;
            _baseRefsAssignViewModel = baseRefsAssignViewModel;
            _opticalEventsDoubleViewModel = opticalEventsDoubleViewModel;
            _commonStatusBarViewModel = commonStatusBarViewModel;
        }

        public void UpdateTrace(object param)
        {
            if (!(param is TraceLeaf traceLeaf))
                return;

            var vm = new TraceInfoViewModel(_readModel, _c2DWcfManager, _windowManager, traceLeaf.Id);
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

        public void ShowTraceEvents(object param)
        {
            if (!(param is TraceLeaf traceLeaf))
                return;

            _reflectogramManager.ShowRftsEventsOfLastTraceMeasurement(traceLeaf.Id);
        }

        public void ShowTraceLandmarks(object param)
        {
            if (!(param is TraceLeaf traceLeaf))
                return;

            var vm = new LandmarksViewModel(_readModel, _windowManager);
            vm.Initialize(traceLeaf.Id, false);
            _windowManager.ShowWindowWithAssignedOwner(vm);
        }

        public void DetachTrace(object param)
        {
            if (!(param is TraceLeaf traceLeaf))
                return;

            _c2DWcfManager.SendCommandAsObj(new DetachTrace() { TraceId = traceLeaf.Id });
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
            var vm = new ConfirmationViewModel(Resources.SID_Confirmation, question);
            _windowManager.ShowDialogWithAssignedOwner(vm);

            if (vm.IsAnswerPositive)
            {
                //                using (new WaitCursor())
                //                {
                _commonStatusBarViewModel.StatusBarMessage2 = Resources.SID_Long_operation__Removing_trace_s_measurements____Please_wait_;
                var cmd = isRemoval ? new RemoveTrace() {Id = traceId} : (object)new CleanTrace() {Id = traceId};
                var result = await _c2DWcfManager.SendCommandAsObj(cmd);
                _commonStatusBarViewModel.StatusBarMessage2 = result ?? "";
                //                }

                _opticalEventsDoubleViewModel.RemoveEventsOfTrace(traceId);
            }
        }

        private static List<ConfirmaionLineModel> AssembleTraceRemovalConfirmation(string traceTitle)
        {
            var list = new List<ConfirmaionLineModel>
            {
                new ConfirmaionLineModel() {Line = Resources.SID_Attention_},
                new ConfirmaionLineModel() {Line = Resources.SID_All_measurements_for_trace},
                new ConfirmaionLineModel() {Line = ""},
                new ConfirmaionLineModel() {Line = $@"{traceTitle}", FontWeight = FontWeights.Bold},
                new ConfirmaionLineModel() {Line = ""},
                new ConfirmaionLineModel() {Line = Resources.SID_will_be_removed},
                new ConfirmaionLineModel() {Line = ""},
                new ConfirmaionLineModel() {Line = Resources.SID_Are_you_sure_},
            };
            return list;
        }

        public void DoPreciseMeasurementOutOfTurn(object param) { }
    }
}