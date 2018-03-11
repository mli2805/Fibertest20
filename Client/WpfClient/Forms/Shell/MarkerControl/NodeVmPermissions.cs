﻿using System.Linq;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Client
{
    public class NodeVmPermissions
    {
        private readonly CurrentUser _currentUser;

        public NodeVmPermissions(CurrentUser currentUser)
        {
            _currentUser = currentUser;
        }

        public bool CanUpdateNode(object parameter)
        {
            return HasPrevilegesAndNotAdjustmentPoint(parameter);
        }

        public bool CanAddEquipment(object parameter)
        {
            return HasPrevilegesAndNotAdjustmentPoint(parameter);
        }

        public bool CanAddCableReserve(object parameter)
        {
            return HasPrevilegesAndNotAdjustmentPoint(parameter);
        }

        public bool CanLandmarks(object parameter) { return false; }

        public bool CanRemoveNode(object parameter)
        {
            if (_currentUser.Role > Role.Root || parameter == null)
                return false;
            var marker = (MarkerControl)parameter;
            return marker.Owner.GraphReadModel.ReadModel.Traces.All(t => t.Nodes.Last() != marker.GMapMarker.Id);
        }

        public bool CanStartAddFiber(object parameter)
        {
            return HasPrevilegesAndNotAdjustmentPoint(parameter);
        }

        public bool CanStartAddFiberWithNodes(object parameter)
        {
            return HasPrevilegesAndNotAdjustmentPoint(parameter);
        }

        private static bool HasPrevilegesAndNotAdjustmentPoint(object parameter)
        {
            if (parameter == null)
                return false;
            var marker = (MarkerControl) parameter;
            return marker.Type != EquipmentType.AdjustmentPoint;
        }
    }
}