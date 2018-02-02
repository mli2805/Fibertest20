using System;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

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
            var equipmentVm = _marker.Owner.GraphReadModel.Equipments.FirstOrDefault(n => n.Node.Id == (Guid)parameter);
            if (equipmentVm == null)
                return false;

            return equipmentVm.Type != EquipmentType.AdjustmentPoint;
        }
        public bool CanAddEquipment(object parameter)
        {
            if (_currentUser.Role > Role.Root || parameter == null)
                return false;
            var equipmentVm = _marker.Owner.GraphReadModel.Equipments.FirstOrDefault(n => n.Node.Id == (Guid)parameter);
            if (equipmentVm == null)
                return false;

            return equipmentVm.Type != EquipmentType.AdjustmentPoint;
        }
        public bool CanAddCableReserve(object parameter)
        {
            if (_currentUser.Role > Role.Root || parameter == null)
                return false;
            var equipmentVm = _marker.Owner.GraphReadModel.Equipments.FirstOrDefault(n => n.Node.Id == (Guid)parameter);
            if (equipmentVm == null)
                return false;

            return equipmentVm.Type != EquipmentType.AdjustmentPoint;
        }
        public bool CanLandmarks(object parameter) { return false; }
        public bool CanRemoveNode(object parameter)
        {
            return parameter != null && _marker.Owner.GraphReadModel.Traces.All(t => t.Nodes.Last() != (Guid)parameter);
        }

        // only for RTU
        public bool CanUpdateRtu(object parameter) { return true; }

        public bool CanRemoveRtu(object parameter)
        {
            if (_currentUser.Role > Role.Root || parameter == null)
                return false;
            var rtuVm = _marker.Owner.GraphReadModel.Rtus.FirstOrDefault(r => r.Node.Id == (Guid) parameter);
            if (rtuVm == null) return false;

            return !_marker.Owner.GraphReadModel.Traces.Any(t => t.RtuId == rtuVm.Id && t.Port > 0);
        }
        public bool CanStartDefineTrace(object parameter) { return true; }
        public bool CanStartDefineTraceStepByStep(object parameter) { return false; }


        // common for RTU and Node
        public bool CanStartAddFiber(object parameter) { return true; }
        public bool CanStartAddFiberWithNodes(object parameter) { return true; }
    }
}