using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Commands;

namespace Iit.Fibertest.TestBench
{
    /// <summary>
        /// Interaction logic for MapUserControl.xaml
        /// </summary>
        public partial class MapUserControl
    {
        public GraphVm GraphVm => (GraphVm)DataContext;

        public MapUserControl()
        {
            InitializeComponent();
            DataContextChanged += MapUserControl_DataContextChanged;
            ConfigureMap();
            // map events
            MainMap.MouseMove += MainMap_MouseMove;
            MainMap.MouseEnter += MainMap_MouseEnter;
        }

        private void ConfigureMap()
        {
            MainMap.MapProvider = GMapProviders.OpenStreetMap;
            MainMap.Position = new PointLatLng(53.856, 27.49);
            MainMap.Zoom = 7;
        }

        private void MapUserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null)
                return;
            var graph = (GraphVm)e.NewValue;

            graph.Nodes.CollectionChanged += Nodes_CollectionChanged;
            graph.Edges.CollectionChanged += Edges_CollectionChanged;
        }

        private void Edges_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    ApplyAddedEdges(e.NewItems);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    ApplyRemovedEdges(e.OldItems);
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

        private void Nodes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
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

        private void ApplyAddedEdges(IList newItems)
        {
            foreach (var newItem in newItems)
            {
                var edgeVm = (FiberVm)newItem;
                var route = new GMapRoute(edgeVm.Id, edgeVm.NodeA.Id, edgeVm.NodeB.Id, Utils.StateToBrush(edgeVm.State),
                    2, new List<PointLatLng>() { edgeVm.NodeA.Position, edgeVm.NodeB.Position });
                route.PropertyChanged += Route_PropertyChanged;
                MainMap.Markers.Add(route);
            }
        }

        private void ApplyRemovedEdges(IList oldItems)
        {
            foreach (var oldItem in oldItems)
            {
                var edgeVm = (FiberVm)oldItem;
                var route = MainMap.Markers.Single(r => r.Id == edgeVm.Id);
                MainMap.Markers.Remove(route);
            }
        }

        private void ApplyAddedNodes(IList newItems)
        {
            foreach (var newItem in newItems)
            {
                var nodeVm = (NodeVm)newItem;
                var marker = new GMapMarker(nodeVm.Id, nodeVm.Position);
                marker.ZIndex = 2;
                var nodePictogram = new MarkerControl(MainMap, marker, nodeVm.Type, nodeVm.Title);
                marker.Shape = nodePictogram;
                MainMap.Markers.Add(marker);

                nodePictogram.PropertyChanged += NodePictogram_PropertyChanged;
            }
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

        private void NodePictogram_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Command")
                GraphVm.Command = ((MarkerControl)sender).Command;
        }

        void MainMap_MouseMove(object sender, MouseEventArgs e)
        {
            var p = e.GetPosition(MainMap);
            GraphVm.CurrentMousePosition =
                MainMap.FromLocalToLatLng((int)p.X, (int)p.Y).ToString();
        }

        void MainMap_MouseEnter(object sender, MouseEventArgs e)
        {
            MainMap.Focus();
        }

        private void AddNodeOnClick(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuItem;
            if (item == null) return;
            var code = int.Parse((string)item.Tag);
            var position = MainMap.FromLocalToLatLng(MainMap.ContextMenuPoint);

            if ((EquipmentType)code == EquipmentType.Rtu)
                GraphVm.Command = new AddRtuAtGpsLocation() { Latitude = position.Lat, Longitude = position.Lng };
            else if ((EquipmentType)code == EquipmentType.Well || (EquipmentType)code == EquipmentType.Invisible)
                GraphVm.Command = new AddNode()
                {
                    Latitude = position.Lat,
                    Longitude = position.Lng,
                    IsJustForCurvature = (EquipmentType)code == EquipmentType.Invisible
                };
            else
                GraphVm.Command = new AddEquipmentAtGpsLocation()
                {
                    Type = (EquipmentType)code,
                    Latitude = position.Lat,
                    Longitude = position.Lng
                };
        }

    }
}