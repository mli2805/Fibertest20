using System;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using GMap.NET.WindowsPresentation;

namespace Iit.Fibertest.TestBench
{
    /// <summary>
    /// Here we react on changes in Nodes collection in GraphVm
    /// applying those to the markers collection of map
    /// </summary>
    public partial class MapUserControl
    {
        private void NodesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    ApplyAddedNodes(e.NewItems);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    ApplyRemovedNodes(e.OldItems);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        private void ApplyAddedNodes(IList newItems)
        {
            foreach (var newItem in newItems)
            {
                var nodeVm = (NodeVm)newItem;
                var marker = new GMapMarker(nodeVm.Id, nodeVm.Position);
                marker.ZIndex = 2;
                var markerControl = new MarkerControl(MainMap, marker, nodeVm.Type, nodeVm.Title);
                marker.Shape = markerControl;
                MainMap.Markers.Add(marker);

                markerControl.PropertyChanged += MarkerControlPropertyChanged;
            }
        }
        private void MarkerControlPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Command")
                GraphVm.Command = ((MarkerControl)sender).Command;
        }

        private void ApplyRemovedNodes(IList oldItems)
        {
            foreach (var oldItem in oldItems)
            {
                var nodeVm = (NodeVm)oldItem;
                var route = MainMap.Markers.Single(r => r.Id == nodeVm.Id);
                MainMap.Markers.Remove(route);
            }
        }
    }
}
