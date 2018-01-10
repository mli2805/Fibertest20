using System;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class MarkerControlActions
    {
        private readonly MarkerControl _marker;

        public MarkerControlActions(MarkerControl marker)
        {
            _marker = marker;
        }

        // only for Node
        public void AskUpdateNode(object parameter)
        {
            var nodeId = (Guid)parameter;
            _marker.Owner.GraphReadModel.Request = new UpdateNode() { Id = nodeId };
        }
        public void AskAddEquipment(object parameter)
        {
            _marker.Owner.GraphReadModel.Request = new RequestAddEquipmentIntoNode() { NodeId = (Guid)parameter };
        }

        public void AskLandmarks(object parameter)
        {
        }

        public void AskRemoveNode(object parameter)
        {
            var nodeId = (Guid)parameter;
            _marker.Owner.GraphReadModel.Request = new RequestRemoveNode() { Id = nodeId };
        }

        // only for RTU
        public void AskUpdateRtu(object parameter)
        {
            var nodeId = (Guid)parameter;
            _marker.Owner.GraphReadModel.Request = new RequestUpdateRtu() { NodeId = nodeId };
        }
        public void AskRemoveRtu(object parameter)
        {
            var nodeId = (Guid)parameter;
            _marker.Owner.GraphReadModel.Request = new RequestRemoveRtu() { NodeId = nodeId };
        }
        public void StartDefineTrace(object parameter)
        {
            _marker.Owner.SetBanner(StringResources.Resources.SID_Trace_definition);
            _marker.MainMap.IsInTraceDefiningMode = true;
            _marker.MainMap.StartNode = _marker.GMapMarker;
        }
        public void StartDefineTraceStepByStep(object parameter)
        {
            _marker.MainMap.IsInTraceDefiningMode = true;
            _marker.MainMap.StartNode = _marker.GMapMarker;
        }

        // common for RTU and Node
        public void StartAddFiber(object parameter)
        {
            _marker.MainMap.IsFiberWithNodes = false;
            _marker.MainMap.IsInFiberCreationMode = true;
            _marker.MainMap.StartNode = _marker.GMapMarker;
        }

        public void StartAddFiberWithNodes(object parameter)
        {
            _marker.MainMap.IsFiberWithNodes = true;
            _marker.MainMap.IsInFiberCreationMode = true;
            _marker.MainMap.StartNode = _marker.GMapMarker;
        }
    }
}
