using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class TraceLeafActionsPermissions
    {
        public bool CanUpdateTrace(object param)
        {
            return true;
        }

        public bool CanShowTrace(object param)
        {
            return true;
        }

        public bool CanAssignBaseRefsAction(object param)
        {
            var traceLeaf = param as TraceLeaf;
            if (traceLeaf == null)
                return false;

            var leaf = traceLeaf.Parent as RtuLeaf;
            var rtuLeaf = leaf ?? (RtuLeaf)traceLeaf.Parent.Parent;

            return traceLeaf.PortNumber < 1
                    || rtuLeaf.IsAvailable &&
                   (traceLeaf.RtuMonitoringState == MonitoringState.Off
                   || !traceLeaf.IsInMonitoringCycle);
        }

        public bool CanShowTraceState(object param)
        {
            return true;
        }

        public bool CanShowTraceStatistics(object param)
        {
            return true;
        }

        public bool CanShowTraceEvents(object param)
        {
            return true;
        }

        public bool CanShowTraceLandmarks(object param)
        {
            return true;
        }

        public bool CanDetachTrace(object param)
        {
            var traceLeaf = param as TraceLeaf;
            if (traceLeaf == null)
                return false;

            return 
                   (traceLeaf.RtuMonitoringState == MonitoringState.Off
                    || !traceLeaf.IsInMonitoringCycle);
        }

        public bool CanCleanTrace(object param)
        {
            return true;
        }

        public bool CanRemoveTrace(object param)
        {
            return true;
        }

        public bool CanDoPreciseMeasurementOutOfTurn(object param)
        {
            return true;
        }

        public bool CanDoMeasurementClient(object param)
        {
            return true;
        }

        public bool CanDoRftsReflectMeasurement(object param)
        {
            return true;
        }
    }
}