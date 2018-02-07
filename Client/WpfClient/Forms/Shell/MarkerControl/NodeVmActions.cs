using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Requests;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class NodeVmActions
    {
        private readonly IWcfServiceForClient _c2DWcfManager;

        public NodeVmActions(IWcfServiceForClient c2DWcfManager)
        {
            _c2DWcfManager = c2DWcfManager;
        }

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
