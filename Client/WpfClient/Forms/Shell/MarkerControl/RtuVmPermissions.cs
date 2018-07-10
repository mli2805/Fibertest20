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
            var readModel = marker.Owner.GraphReadModel.ReadModel;
            var rtuVm = readModel.Rtus.FirstOrDefault(r => r.NodeId == marker.GMapMarker.Id);
            if (rtuVm == null) return false;

            return (!readModel.Traces.Any(t => t.RtuId == rtuVm.Id && t.Port > 0)
                || !readModel.Rtus.First(r=>r.Id == rtuVm.Id).IsAvailable);
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
            var rtuVm = marker.Owner.GraphReadModel.ReadModel.Rtus.FirstOrDefault(r => r.NodeId == marker.GMapMarker.Id);
            return rtuVm != null;
        }

        public bool CanHideTraces(object _) { return true; }
    }
}