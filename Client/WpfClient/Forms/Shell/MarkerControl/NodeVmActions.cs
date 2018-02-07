using System;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly GraphReadModel _graphReadModel;
        private readonly IWcfServiceForClient _c2DWcfManager;

        public NodeVmActions(ILifetimeScope globalScope, IWindowManager windowManager, GraphReadModel graphReadModel, IWcfServiceForClient c2DWcfManager)
        {
            _globalScope = globalScope;
            _windowManager = windowManager;
            _graphReadModel = graphReadModel;
            _c2DWcfManager = c2DWcfManager;
        }

        public void AskUpdateNode(object parameter)
        {
            var marker = (MarkerControl)parameter;
            var vm = _globalScope.Resolve<NodeUpdateViewModel>();
            vm.Initialize(marker.GMapMarker.Id);
            _windowManager.ShowDialogWithAssignedOwner(vm);
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

        public async void AskRemoveNode(object parameter)
        {
            var marker = (MarkerControl)parameter;
            await RemoveNode(marker.GMapMarker.Id);
        }

        public async Task RemoveNode(Guid nodeId)
        {
            if (_graphReadModel.Traces.Any(t => t.Nodes.Last() == nodeId))
                return;
            if (_graphReadModel.Traces.Any(t => t.Nodes.Contains(nodeId) && t.HasBase))
                return;

            var dictionary = _graphReadModel.Traces.Where(t => t.Nodes.Contains(nodeId))
                .ToDictionary(trace => trace.Id, trace => Guid.NewGuid());
            var cmd = new RemoveNode {Id = nodeId, TraceWithNewFiberForDetourRemovedNode = dictionary};
            var message = await _c2DWcfManager.SendCommandAsObj(cmd);
            if (message != null)
                _windowManager.ShowDialogWithAssignedOwner(new MyMessageBoxViewModel(MessageType.Error, message));
        }
    }
}
