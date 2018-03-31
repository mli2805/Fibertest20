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

        public bool CanUpdateRtu(object parameter)
        {
            return HasPriveligesAndParameterValid(parameter);
        }

        public bool CanRemoveRtu(object parameter)
        {
            if (_currentUser.Role > Role.Root || parameter == null)
                return false;
            var marker = (MarkerControl)parameter;
            var rtuVm = marker.Owner.GraphReadModel.Model.Rtus.FirstOrDefault(r => r.NodeId == marker.GMapMarker.Id);
            if (rtuVm == null) return false;

            return !marker.Owner.GraphReadModel.Model.Traces.Any(t => t.RtuId == rtuVm.Id && t.Port > 0);
        }

        public bool CanStartAddFiber(object parameter)
        {
            return HasPriveligesAndParameterValid(parameter);
        }

        public bool CanStartAddFiberWithNodes(object parameter)
        {
            return HasPriveligesAndParameterValid(parameter);
        }

        public bool CanStartDefineTrace(object parameter)
        {
            return HasPriveligesAndParameterValid(parameter);
        }

        public bool CanStartDefineTraceStepByStep(object parameter)
        {
            return HasPriveligesAndParameterValid(parameter);
        }

        private bool HasPriveligesAndParameterValid(object parameter)
        {
            if (_currentUser.Role > Role.Root || parameter == null)
                return false;
            var marker = (MarkerControl) parameter;
            var rtuVm = marker.Owner.GraphReadModel.Model.Rtus.FirstOrDefault(r => r.NodeId == marker.GMapMarker.Id);
            return rtuVm != null;
        }
    }
}