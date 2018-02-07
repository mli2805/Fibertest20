namespace Iit.Fibertest.Client
{
    public class CommonVmActions
    {
        public void StartAddFiber(object parameter)
        {
            var marker = (MarkerControl)parameter;
            marker.MainMap.IsFiberWithNodes = false;
            marker.MainMap.IsInFiberCreationMode = true;
            marker.MainMap.StartNode = marker.GMapMarker;
        }

        public void StartAddFiberWithNodes(object parameter)
        {
            var marker = (MarkerControl)parameter;
            marker.MainMap.IsFiberWithNodes = true;
            marker.MainMap.IsInFiberCreationMode = true;
            marker.MainMap.StartNode = marker.GMapMarker;
        }
    }
}