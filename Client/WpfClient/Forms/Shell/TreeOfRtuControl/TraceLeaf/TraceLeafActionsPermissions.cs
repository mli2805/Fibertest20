using System;
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

        public bool CanShowTrace(object param)
        {
            if (!(param is TraceLeaf traceLeaf))
                return false;

            return traceLeaf.IsInZone;
        }

        // if trace attached to port and RTU is not available now - it is prohibited to assign base - you can't send base to RTU
        public bool CanAssignBaseRefsAction(object param)
        {
            if (_currentUser.Role > Role.Root)
                return false;
            if (!(param is TraceLeaf traceLeaf) || !traceLeaf.IsInZone)
                return false;


            var leaf = traceLeaf.Parent as RtuLeaf;
            var rtuLeaf = leaf ?? (RtuLeaf)traceLeaf.Parent.Parent;

            if (rtuLeaf.TreeOfAcceptableMeasParams == null) // RTU is not initialized yet
                return false;

            return traceLeaf.PortNumber < 1
                    || rtuLeaf.IsAvailable &&
                   (traceLeaf.BaseRefsSet.RtuMonitoringState == MonitoringState.Off
                   || !traceLeaf.BaseRefsSet.IsInMonitoringCycle);
        }

        public bool CanShowTraceState(object param)
        {
            if (!(param is TraceLeaf traceLeaf))
                return false;

            return traceLeaf.IsInZone;
        }

        public bool CanShowTraceStatistics(object param)
        {
            if (!(param is TraceLeaf traceLeaf))
                return false;

            return traceLeaf.IsInZone; return true;
        }

        public bool CanShowTraceLandmarks(object param)
        {
            if (!(param is TraceLeaf traceLeaf))
                return false;

            return traceLeaf.IsInZone;
        }

        public bool CanDetachTrace(object param)
        {
            if (_currentUser.Role > Role.Root)
                return false;
            if (!(param is TraceLeaf traceLeaf) || !traceLeaf.IsInZone)
                return false;

            return traceLeaf.BaseRefsSet.RtuMonitoringState == MonitoringState.Off
                                || !traceLeaf.BaseRefsSet.IsInMonitoringCycle;
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
            if (_currentUser.Role > Role.Operator)
                return false;

            if (!(param is TraceLeaf traceLeaf) || !traceLeaf.IsInZone)
                return false;

            return traceLeaf.PortNumber > 0 && traceLeaf.BaseRefsSet.PreciseId != Guid.Empty;
        }
    }
}