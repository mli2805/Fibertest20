using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Graph.Requests;

namespace Iit.Fibertest.Client
{
    public class NodeVmActions
    {
        private readonly ILifetimeScope _globalScope;
        private readonly IWindowManager _windowManager;

        public NodeVmActions(ILifetimeScope globalScope, IWindowManager windowManager)
        {
            _globalScope = globalScope;
            _windowManager = windowManager;
        }

        public void AskUpdateNode(object parameter)
        {
            var marker = (MarkerControl)parameter;
            var vm = _globalScope.Resolve<NodeUpdateViewModel>();
            vm.Initialize(marker.GMapMarker.Id);
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }
        public async void AskAddEquipment(object parameter)
        {
            var marker = (MarkerControl)parameter;
            await marker.Owner.GraphReadModel.GrmEquipmentRequests.
                AddEquipmentIntoNode(new RequestAddEquipmentIntoNode()
                    { NodeId = marker.GMapMarker.Id, IsCableReserveRequested = false });
        }

        public async void AskAddCableReserve(object parameter)
        {
            var marker = (MarkerControl)parameter;
            await marker.Owner.GraphReadModel.GrmEquipmentRequests.
                AddEquipmentIntoNode(new RequestAddEquipmentIntoNode()
                    { NodeId = marker.GMapMarker.Id, IsCableReserveRequested = true });
        }

        public void AskLandmarks(object parameter)
        {
        }

        public async void AskRemoveNode(object parameter)
        {
            var marker = (MarkerControl)parameter;
            await marker.Owner.GraphReadModel.GrmNodeRequests.RemoveNode(marker.GMapMarker.Id, marker.Type);
        }
    }
}
