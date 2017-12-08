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
        private readonly TraceStateViewsManager _traceStateViewsManager;
        private readonly TraceStatisticsViewsManager _traceStatisticsViewsManager;
        private readonly ReflectogramManager _reflectogramManager;

        public TraceLeafActions(IniFile iniFile, IMyLog logFile,
            TraceStateViewsManager traceStateViewsManager, TraceStatisticsViewsManager traceStatisticsViewsManager,
             ReflectogramManager reflectogramManager)
        {
            _iniFile = iniFile;
            _logFile = logFile;
            _traceStateViewsManager = traceStateViewsManager;
            _traceStatisticsViewsManager = traceStatisticsViewsManager;
            _reflectogramManager = reflectogramManager;
        }

        public void UpdateTrace(object param)
        {
            var traceLeaf = param as TraceLeaf;
            if (traceLeaf == null)
                return;

            var vm = new TraceInfoViewModel(traceLeaf.ReadModel, traceLeaf.C2DWcfManager, traceLeaf.WindowManager, traceLeaf.Id);
            traceLeaf.WindowManager.ShowWindow(vm);
        }

        public void ShowTrace(object param)
        {
        }

        // if trace attached to port and rtu is not available now - it is prohibited to assign base - you can't send base to rtu
        public void AssignBaseRefs(object param)
        {
            var traceLeaf = param as TraceLeaf;
            if (traceLeaf == null)
                return;

            var trace = traceLeaf.ReadModel.Traces.First(t => t.Id == traceLeaf.Id);
            var vm = new BaseRefsAssignViewModel(_iniFile, traceLeaf.ReadModel, traceLeaf.C2DWcfManager, traceLeaf.WindowManager, new SorExt(_logFile));
            vm.Initialize(trace);
            traceLeaf.WindowManager.ShowWindow(vm);
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
            traceLeaf.WindowManager.ShowWindow(vm);
        }

        public void DetachTrace(object param)
        {
            var traceLeaf = param as TraceLeaf;
            if (traceLeaf == null)
                return;

            traceLeaf.C2DWcfManager.SendCommandAsObj(new DetachTrace() { TraceId = traceLeaf.Id });

        }

        public void CleanTrace(object param)
        {
            var traceLeaf = param as TraceLeaf;
            if (traceLeaf == null)
                return;

            traceLeaf.C2DWcfManager.SendCommandAsObj(new CleanTrace() { Id = traceLeaf.Id });
        }

        public void RemoveTrace(object param)
        {
            var traceLeaf = param as TraceLeaf;
            if (traceLeaf == null)
                return;

            traceLeaf.C2DWcfManager.SendCommandAsObj(new RemoveTrace() { Id = traceLeaf.Id });
        }
        public void DoPreciseMeasurementOutOfTurn(object param) { }

        public void DoRftsReflectMeasurement(object param) { }

    }
}