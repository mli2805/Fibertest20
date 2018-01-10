using System;
using System.Linq;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Client
{
    public class MarkerControlPermissions
    {
        private readonly CurrentUser _currentUser;
        private readonly MarkerControl _marker;

        public MarkerControlPermissions(CurrentUser currentUser, MarkerControl marker)
        {
            _currentUser = currentUser;
            _marker = marker;
        }

        // only for Node
        public bool CanUpdateNode(object parameter)
        {
            if (parameter == null)
                return false;
            var nodeVm = _marker.Owner.GraphReadModel.Nodes.FirstOrDefault(n => n.Id == (Guid)parameter);
            if (nodeVm == null)
                return false;

            return !nodeVm.IsAdjustmentNode;
        }
        public bool CanAddEquipment(object parameter)
        {
            if (_currentUser.Role > Role.Root || parameter == null)
                return false;
            var nodeVm = _marker.Owner.GraphReadModel.Nodes.FirstOrDefault(n => n.Id == (Guid)parameter);
            if (nodeVm == null)
                return false;

            return !nodeVm.IsAdjustmentNode;
        }
        public bool CanLandmarks(object parameter) { return false; }
        public bool CanRemoveNode(object parameter)
        {
            return parameter != null && _marker.Owner.GraphReadModel.Traces.All(t => t.Nodes.Last() != (Guid)parameter);
        }

        // only for RTU
        public bool CanUpdateRtu(object parameter) { return true; }
        public bool CanRemoveRtu(object parameter) { return true; }
        public bool CanStartDefineTrace(object parameter) { return true; }
        public bool CanStartDefineTraceStepByStep(object parameter) { return false; }


        // common for RTU and Node
        public bool CanStartAddFiber(object parameter) { return true; }
        public bool CanStartAddFiberWithNodes(object parameter) { return true; }
    }
}