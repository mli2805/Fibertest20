using Iit.Fibertest.Graph.Requests;

namespace Iit.Fibertest.Client
{
    public class RtuVmActions
    {
        public void AskUpdateRtu(object parameter)
        {
            var marker = (MarkerControl)parameter;
            marker.Owner.GraphReadModel.Request = new RequestUpdateRtu() { NodeId = marker.GMapMarker.Id };
        }
        public void AskRemoveRtu(object parameter)
        {
            var marker = (MarkerControl)parameter;
            marker.Owner.GraphReadModel.Request = new RequestRemoveRtu() { NodeId = marker.GMapMarker.Id };
        }
        public void StartDefineTrace(object parameter)
        {
            var marker = (MarkerControl)parameter;
            marker.Owner.SetBanner(StringResources.Resources.SID_Trace_definition);
            marker.MainMap.IsInTraceDefiningMode = true;
            marker.MainMap.StartNode = marker.GMapMarker;
        }
        public void StartDefineTraceStepByStep(object parameter)
        {
            var marker = (MarkerControl)parameter;
            marker.MainMap.IsInTraceDefiningMode = true;
            marker.MainMap.StartNode = marker.GMapMarker;
        }

    }
}