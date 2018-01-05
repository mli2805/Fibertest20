using System;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Client
{
    public class TraceLeafActions
    {
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;
        private readonly IWindowManager _windowManager;
        private readonly TraceStateViewsManager _traceStateViewsManager;
        private readonly TraceStatisticsViewsManager _traceStatisticsViewsManager;
        private readonly ReflectogramManager _reflectogramManager;
        private readonly BaseRefsAssignViewModel _baseRefsAssignViewModel;
        private readonly OpticalEventsDoubleViewModel _opticalEventsDoubleViewModel;
        private readonly CommonStatusBarViewModel _commonStatusBarViewModel;

        public TraceLeafActions(IniFile iniFile, IMyLog logFile, IWindowManager windowManager,
            TraceStateViewsManager traceStateViewsManager, TraceStatisticsViewsManager traceStatisticsViewsManager,
             ReflectogramManager reflectogramManager, BaseRefsAssignViewModel baseRefsAssignViewModel,
            OpticalEventsDoubleViewModel opticalEventsDoubleViewModel,
            CommonStatusBarViewModel commonStatusBarViewModel)
        {
            _iniFile = iniFile;
            _logFile = logFile;
            _windowManager = windowManager;
            _traceStateViewsManager = traceStateViewsManager;
            _traceStatisticsViewsManager = traceStatisticsViewsManager;
            _reflectogramManager = reflectogramManager;
            _baseRefsAssignViewModel = baseRefsAssignViewModel;
            _opticalEventsDoubleViewModel = opticalEventsDoubleViewModel;
            _commonStatusBarViewModel = commonStatusBarViewModel;
        }

        public void UpdateTrace(object param)
        {
            var traceLeaf = param as TraceLeaf;
            if (traceLeaf == null)
                return;

            var vm = new TraceInfoViewModel(traceLeaf.ReadModel, traceLeaf.C2DWcfManager, traceLeaf.WindowManager, traceLeaf.Id);
            traceLeaf.WindowManager.ShowWindowWithAssignedOwner(vm);
        }

        public void ShowTrace(object param)
        {
        }

        // if trace attached to port and RTU is not available now - it is prohibited to assign base - you can't send base to RTU
        public void AssignBaseRefs(object param)
        {
            if (!(param is TraceLeaf traceLeaf))
                return;

            var trace = traceLeaf.ReadModel.Traces.First(t => t.Id == traceLeaf.Id);
            //            var vm = new BaseRefsAssignViewModel(_iniFile, traceLeaf.ReadModel, traceLeaf.C2DWcfManager, traceLeaf.WindowManager, new BaseRefDtoFactory(_logFile));
            //            vm.Initialize(trace);
            //            traceLeaf.WindowManager.ShowWindowWithAssignedOwner(vm);
            _baseRefsAssignViewModel.Initialize(trace);
            traceLeaf.WindowManager.ShowDialogWithAssignedOwner(_baseRefsAssignViewModel);
        }

        public void ShowTraceState(object param)
        {
            var traceLeaf = param as TraceLeaf;
            if (traceLeaf == null)
                return;

            _traceStateViewsManager.ShowTraceState(traceLeaf.Id);
        }

        public void ShowTraceStatistics(object param)
        {
            var traceLeaf = param as TraceLeaf;
            if (traceLeaf == null)
                return;

            _traceStatisticsViewsManager.Show(traceLeaf.Id);
        }

        public void ShowTraceEvents(object param)
        {
            var traceLeaf = param as TraceLeaf;
            if (traceLeaf == null)
                return;

            _reflectogramManager.ShowRftsEventsOfLastTraceMeasurement(traceLeaf.Id);
        }

        public void ShowTraceLandmarks(object param)
        {
            var traceLeaf = param as TraceLeaf;
            if (traceLeaf == null)
                return;

            var vm = new LandmarksViewModel(traceLeaf.ReadModel, traceLeaf.WindowManager);
            vm.Initialize(traceLeaf.Id, false);
            traceLeaf.WindowManager.ShowWindowWithAssignedOwner(vm);
        }

        public void DetachTrace(object param)
        {
            var traceLeaf = param as TraceLeaf;
            if (traceLeaf == null)
                return;

            traceLeaf.C2DWcfManager.SendCommandAsObj(new DetachTrace() { TraceId = traceLeaf.Id });
        }

        public async void CleanTrace(object param)
        {
            DoCleanOrRemoveTrace(param, false);
        }

        public void RemoveTrace(object param)
        {
           DoCleanOrRemoveTrace(param, true);
        }

        private async void DoCleanOrRemoveTrace(object param, bool isRemoval)
        {
            if (!(param is TraceLeaf traceLeaf))
                return;
            var traceId = traceLeaf.Id;

            var message = $"Attention!{Environment.NewLine}" +
                          $"All measurements for trace{Environment.NewLine}" +
                          $"{traceLeaf.Title}{Environment.NewLine}{Environment.NewLine}" +
                          $"will be removed{Environment.NewLine}{Environment.NewLine}" +
                          "Are you sure?";
            var vm = new QuestionViewModel(message);
            _windowManager.ShowDialogWithAssignedOwner(vm);
            if (vm.IsAnswerPositive)
            {
                //                using (new WaitCursor())
                //                {
                _commonStatusBarViewModel.StatusBarMessage2 = "Long operation: Removing trace measurements... Please wait.";
                var cmd = isRemoval ? new RemoveTrace() {Id = traceId} : (object)new CleanTrace() {Id = traceId};
                var result = await traceLeaf.C2DWcfManager.SendCommandAsObj(cmd);
                _commonStatusBarViewModel.StatusBarMessage2 = result ?? "";
                //                }

                _opticalEventsDoubleViewModel.RemoveEventsOfTrace(traceId);
            }
        }
        public void DoPreciseMeasurementOutOfTurn(object param) { }

        public void DoRftsReflectMeasurement(object param) { }

    }
}