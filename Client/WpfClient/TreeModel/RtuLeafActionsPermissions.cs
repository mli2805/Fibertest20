using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class RtuLeafActionsPermissions
    {
        public bool CanUpdateRtu(object param)
        {
            return true;
        }

        public bool CanShowRtu(object param)
        {
            return true;
        }

        public bool CanInitializeRtu(object param)
        {
            return true;
        }

        public bool CanShowRtuState(object param)
        {
            return true;
        }

        public bool CanShowRtuLandmarks(object param)
        {
            return true;
        }

        public bool CanShowMonitoringSettings(object param)
        {
            var rtuLeaf = param as RtuLeaf;
            return rtuLeaf != null && rtuLeaf.IsAvailable;
        }

        public bool CanStartMonitoring(object param)
        {
            var rtuLeaf = param as RtuLeaf;
            return rtuLeaf != null && rtuLeaf.IsAvailable && rtuLeaf.MonitoringState == MonitoringState.Off;
        }

        public bool CanStopMonitoring(object param)
        {
            var rtuLeaf = param as RtuLeaf;
            return rtuLeaf != null && rtuLeaf.IsAvailable && rtuLeaf.MonitoringState == MonitoringState.On;
        }

        public bool CanRemoveRtu(object param)
        {
            var rtuLeaf = param as RtuLeaf;
            return rtuLeaf != null && !rtuLeaf.HasAttachedTraces;
        }

        public bool CanDefineTraceStepByStep(object param)
        {
            return true;
        }

    }
}