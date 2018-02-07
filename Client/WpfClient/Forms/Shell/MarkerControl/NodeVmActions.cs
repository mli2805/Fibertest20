using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Requests;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class NodeVmActions
    {
        private readonly ILifetimeScope _globalScope;
        private readonly IWindowManager _windowManager;
        private readonly IWcfServiceForClient _c2DWcfManager;

        public NodeVmActions(ILifetimeScope globalScope, IWindowManager windowManager, IWcfServiceForClient c2DWcfManager)
        {
            _globalScope = globalScope;
            _windowManager = windowManager;
            _c2DWcfManager = c2DWcfManager;
        }

        public void AskUpdateNode(object parameter)
        {
            var marker = (MarkerControl)parameter;

            var vm = _globalScope.Resolve<NodeUpdateViewModel>();
            vm.Initialize(marker.GMapMarker.Id);
            _windowManager.ShowDialogWithAssignedOwner(vm);


//            marker.Owner.GraphReadModel.Request = new UpdateNode() { Id = marker.GMapMarker.Id };
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
