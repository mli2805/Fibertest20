﻿using System;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using GMap.NET.WindowsPresentation;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Client
{
    /// <summary>
    /// Here we react on changes in Nodes collection in GraphReadModel
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
                if (nodeVm.Id.ToString().StartsWith(@"61c04"))
                {
                    Console.WriteLine(@"ddd");
                }
                nodeVm.PropertyChanged += NodeVm_PropertyChanged;
                var marker = new GMapMarker(nodeVm.Id, nodeVm.Position, false);
                marker.ZIndex = nodeVm.Type == EquipmentType.AccidentPlace ? -2 : 2;
                var equipmentType = nodeVm.Type;
                var markerControl = new MarkerControl(this, marker, equipmentType, nodeVm.State, nodeVm.Title, GraphReadModel.GlobalScope);
                marker.Shape = markerControl;
                marker.Shape.Visibility =
                    GraphReadModel.SelectedGraphVisibilityItem.Level >= ((MarkerControl)marker.Shape).EqType.GetEnabledVisibilityLevel()
                        ? Visibility.Visible
                        : Visibility.Hidden;
                MainMap.Markers.Add(marker);
            }
        }

        private void NodeVm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var nodeVm = (NodeVm)sender;

            if (e.PropertyName == @"Position")
            {
                MainMap.Markers.First(m => m.Id == nodeVm.Id).Position = nodeVm.Position;
                foreach (var route in MainMap.Markers.OfType<GMapRoute>().Where(r => r.LeftId == nodeVm.Id))
                {
                    route.Points[0] = nodeVm.Position;
                    route.RegenerateShape(MainMap);
                }
                foreach (var route in MainMap.Markers.OfType<GMapRoute>().Where(r => r.RightId == nodeVm.Id))
                {
                    route.Points[1] = nodeVm.Position;
                    route.RegenerateShape(MainMap);
                }
            }
            if (e.PropertyName == @"Title")
            {
                ((MarkerControl)MainMap.Markers.First(m => m.Id == nodeVm.Id).Shape).Title = nodeVm.Title;
            }
            if (e.PropertyName == @"Type")
            {
                ((MarkerControl)MainMap.Markers.First(m => m.Id == nodeVm.Id).Shape).Type = nodeVm.Type;
            }

            if (e.PropertyName == @"IsHighlighted")
            {
                if (nodeVm.IsHighlighted) Highlight(nodeVm);
                else
                    Extinguish();
            }
        }

        private void Highlight(NodeVm nodeVm)
        {
            var marker = new GMapMarker(nodeVm.Id, nodeVm.Position, true) {ZIndex = -20000};

            var highlightingControl = nodeVm.Type == EquipmentType.Rtu ? new HighlightingRtuControl() : (UIElement)new HighlightingControl();
            marker.Shape = highlightingControl;
            marker.Offset = nodeVm.Type == EquipmentType.Rtu || nodeVm.Type == EquipmentType.AccidentPlace 
                ? new Point(-24, -24) 
                : new Point(-16, -16);
            MainMap.Markers.Add(marker);
        }

        private void Extinguish()
        {
            var marker = MainMap.Markers.FirstOrDefault(m => m.IsHighlighting);
            MainMap.Markers.Remove(marker);
        }

        private void ApplyRemovedNodes(IList oldItems)
        {
            foreach (var oldItem in oldItems)
            {
                var nodeVm = (NodeVm)oldItem;
                var marker = MainMap.Markers.First(r => r.Id == nodeVm.Id);
                MainMap.Markers.Remove(marker);

            }
        }
    }
}
