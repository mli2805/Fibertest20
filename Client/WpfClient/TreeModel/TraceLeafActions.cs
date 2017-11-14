using System.Linq;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public class TraceLeafActions
    {
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;

        public TraceLeafActions(IniFile iniFile, IMyLog logFile)
        {
            _iniFile = iniFile;
            _logFile = logFile;
        }

        public void UpdateTrace(object param)
        {
            var traceLeaf = param as TraceLeaf;
            if (traceLeaf == null)
                return;

            var vm = new TraceInfoViewModel(traceLeaf.ReadModel, traceLeaf.C2DWcfManager, traceLeaf.WindowManager, traceLeaf.Id);
            traceLeaf.WindowManager.ShowDialog(vm);
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
            traceLeaf.WindowManager.ShowDialog(vm);
        }

        public void ShowTraceState(object param) { }
        public void ShowTraceStatistics(object param) { }

        public void ShowTraceEvents(object param)
        {
            var traceLeaf = param as TraceLeaf;
            if (traceLeaf == null)
                return;

            //TODO get last measurement sordata and pass it to vm
            //            if (sorData.RftsEvents.MonitoringResult == (int)ComparisonReturns.NoFiber)
            //            {
            //                MessageBox.Show("No Fiber!", "Events");
            //                return;
            //            }
            var vm = new RftsEventsViewModel();
            traceLeaf.WindowManager.ShowDialog(vm);
        }

        public void ShowTraceLandmarks(object param)
        {
            var traceLeaf = param as TraceLeaf;
            if (traceLeaf == null)
                return;

            var vm = new LandmarksViewModel(traceLeaf.ReadModel, traceLeaf.WindowManager);
            vm.Initialize(traceLeaf.Id, false);
            traceLeaf.WindowManager.ShowDialog(vm);
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