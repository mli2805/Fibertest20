using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class RtuLeafActionsPermissions
    {
        private readonly CurrentUser _currentUser;
        private readonly Model _readModel;

        public RtuLeafActionsPermissions(CurrentUser currentUser, Model readModel)
        {
            _currentUser = currentUser;
            _readModel = readModel;
        }

        public bool CanUpdateRtu(object param)
        {
            return _currentUser.Role <= Role.Root;
        }

        public bool CanShowRtu(object param) { return true; }

        public bool CanInitializeRtu(object param)
        {
            return _currentUser.Role <= Role.Root && param is RtuLeaf;
        }

        public bool CanShowRtuState(object param)
        {
            return param is RtuLeaf rtuLeaf && rtuLeaf.MainChannelState != RtuPartState.NotSetYet;
        }

        public bool CanShowRtuLandmarks(object param)
        {
            if (!(param is RtuLeaf rtuLeaf)) return false;
            return _readModel.Traces.Any(t => t.RtuId == rtuLeaf.Id);
        }

        public bool CanShowMonitoringSettings(object param)
        {
            return param is RtuLeaf rtuLeaf && rtuLeaf.IsAvailable;
        }

        public bool CanStartMonitoring(object param)
        {
            return _currentUser.Role <= Role.Operator 
                   && param is RtuLeaf rtuLeaf 
                   && rtuLeaf.IsAvailable 
                   && rtuLeaf.MonitoringState == MonitoringState.Off;
        }

        public bool CanStopMonitoring(object param)
        {
            return _currentUser.Role <= Role.Operator
                   && param is RtuLeaf rtuLeaf
                   && rtuLeaf.IsAvailable
                   && rtuLeaf.MonitoringState == MonitoringState.On;
        }

        public bool CanRemoveRtu(object param)
        {
            return _currentUser.Role <= Role.Root 
                   && param is RtuLeaf rtuLeaf 
                   && (!rtuLeaf.HasAttachedTraces || !rtuLeaf.IsAvailable);
        }

        public bool CanDefineTraceStepByStep(object param)
        {
            return _currentUser.Role <= Role.Root;
        }

        public bool CanHideTraces(object _)
        {
            return true;
        }
    }
}