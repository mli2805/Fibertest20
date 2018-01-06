using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Client
{
    public class TraceLeafActionsPermissions
    {
        private readonly CurrentUser _currentUser;

        public TraceLeafActionsPermissions(CurrentUser currentUser)
        {
            _currentUser = currentUser;
        }

        public bool CanUpdateTrace(object param)
        {
            return _currentUser.Role <= Role.Root;
        }

        public bool CanShowTrace(object param) { return true; }

        // if trace attached to port and RTU is not available now - it is prohibited to assign base - you can't send base to RTU
        public bool CanAssignBaseRefsAction(object param)
        {
            if (_currentUser.Role > Role.Root)
                return false;
            if (!(param is TraceLeaf traceLeaf))
                return false;

            var leaf = traceLeaf.Parent as RtuLeaf;
            var rtuLeaf = leaf ?? (RtuLeaf)traceLeaf.Parent.Parent;

            return traceLeaf.PortNumber < 1
                    || rtuLeaf.IsAvailable &&
                   (traceLeaf.TraceMonitoringState == MonitoringState.Off
                   || !traceLeaf.IsInMonitoringCycle);
        }

        public bool CanShowTraceState(object param) { return true; }

        public bool CanShowTraceStatistics(object param) { return true; }

        public bool CanShowTraceEvents(object param) { return true; }

        public bool CanShowTraceLandmarks(object param) { return true; }

        public bool CanDetachTrace(object param)
        {
            if (!(param is TraceLeaf traceLeaf))
                return false;
            if (_currentUser.Role > Role.Root)
                return false;
            
            return traceLeaf.TraceMonitoringState == MonitoringState.Off
                                || !traceLeaf.IsInMonitoringCycle;
        }

        public bool CanCleanTrace(object param)
        {
            return _currentUser.Role <= Role.Root;
        }

        public bool CanRemoveTrace(object param)
        {
            return _currentUser.Role <= Role.Root;
        }

        public bool CanDoPreciseMeasurementOutOfTurn(object param)
        {
            return _currentUser.Role <= Role.Operator;
        }
    }
}