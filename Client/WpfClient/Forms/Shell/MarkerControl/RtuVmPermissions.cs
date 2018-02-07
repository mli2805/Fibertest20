using System.Linq;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Client
{
    public class RtuVmPermissions
    {
        private readonly CurrentUser _currentUser;

        public RtuVmPermissions(CurrentUser currentUser)
        {
            _currentUser = currentUser;
        }

        public bool CanUpdateRtu(object parameter) { return true; }

        public bool CanRemoveRtu(object parameter)
        {
            if (_currentUser.Role > Role.Root || parameter == null)
                return false;
            var marker = (MarkerControl)parameter;
            var rtuVm = marker.Owner.GraphReadModel.Rtus.FirstOrDefault(r => r.Node.Id == marker.GMapMarker.Id);
            if (rtuVm == null) return false;

            return !marker.Owner.GraphReadModel.Traces.Any(t => t.RtuId == rtuVm.Id && t.Port > 0);
        }
        public bool CanStartAddFiber(object parameter)
        {
            if (_currentUser.Role > Role.Root || parameter == null)
                return false;
            var marker = (MarkerControl)parameter;
            var rtuVm = marker.Owner.GraphReadModel.Rtus.FirstOrDefault(n => n.Node.Id == marker.GMapMarker.Id);
            return rtuVm != null;
        }

        public bool CanStartAddFiberWithNodes(object parameter)
        {
            if (_currentUser.Role > Role.Root || parameter == null)
                return false;
            var marker = (MarkerControl)parameter;
            var rtuVm = marker.Owner.GraphReadModel.Rtus.FirstOrDefault(n => n.Node.Id == marker.GMapMarker.Id);
            return rtuVm != null;
        }

        public bool CanStartDefineTrace(object parameter) { return true; }
        public bool CanStartDefineTraceStepByStep(object parameter) { return false; }
    }
}