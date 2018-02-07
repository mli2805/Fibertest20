using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Requests;

namespace Iit.Fibertest.Client
{
    public class NodeVmActions
    {
        public void AskUpdateNode(object parameter)
        {
            var marker = (MarkerControl)parameter;
            marker.Owner.GraphReadModel.Request = new UpdateNode() { Id = marker.GMapMarker.Id };
        }
        public void AskAddEquipment(object parameter)
        {
            var marker = (MarkerControl)parameter;
            marker.Owner.GraphReadModel.Request = new RequestAddEquipmentIntoNode() { NodeId = marker.GMapMarker.Id, IsCableReserveRequested = false };
        }

        public void AskAddCableReserve(object parameter)
        {
            var marker = (MarkerControl)parameter;
            marker.Owner.GraphReadModel.Request = new RequestAddEquipmentIntoNode() { NodeId = marker.GMapMarker.Id, IsCableReserveRequested = true };
        }

        public void AskLandmarks(object parameter)
        {
        }

        public void AskRemoveNode(object parameter)
        {
            var marker = (MarkerControl)parameter;
            marker.Owner.GraphReadModel.Request = new RequestRemoveNode() { Id = marker.GMapMarker.Id };
        }
    }
}
