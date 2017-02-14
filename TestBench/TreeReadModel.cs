﻿using System;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Iit.Fibertest.Graph.Events;

namespace Iit.Fibertest.TestBench
{
    public class TreeReadModel
    {
        public ObservableCollection<Leaf> Tree { get; set; } = new ObservableCollection<Leaf>();

        #region Node
        public void Apply(NodeAdded e)
        {
        }

        public void Apply(NodeIntoFiberAdded e)
        {
        }

        public void Apply(NodeUpdated source)
        {
        }

        public void Apply(NodeMoved newLocation)
        {
        }

        public void Apply(NodeRemoved e)
        {
        }
        #endregion

        #region Fiber
        public void Apply(FiberAdded e)
        {
        }

        public void Apply(FiberUpdated source)
        {
        }

        public void Apply(FiberRemoved e)
        {
        }
        #endregion

        #region Equipment
        public void Apply(EquipmentAdded e)
        {
        }

        public void Apply(EquipmentAtGpsLocationAdded e)
        {
        }

        public void Apply(EquipmentUpdated e)
        {
        }

        public void Apply(EquipmentRemoved e)
        {
        }
        #endregion

        #region Rtu
        public void Apply(RtuAtGpsLocationAdded e)
        {
            var path1 = "pack://application:,,,/Resources/LeftPanel/wifi_green.png";
            var imageSource1 = new BitmapImage(new Uri(path1));
            var path2 = "pack://application:,,,/Resources/LeftPanel/Monitoring_Blue.png";
            var imageSource2 = new BitmapImage(new Uri(path2));
            var path4 = "pack://application:,,,/Resources/LeftPanel/Red_Ball_16.jpg";
            var imageSource4 = new BitmapImage(new Uri(path4));

            var leaf = new Leaf()
            {
                Id = e.Id, LeafType = LeafType.Rtu, Title = "noname RTU", Color = Brushes.DarkGray,
                Pic1 = imageSource2, Pic2 = imageSource1, Pic4 = imageSource4
            };
            Tree.Add(leaf);
        }

        public void Apply(RtuUpdated e)
        {
            var rtu = Tree.GetById(e.Id);
            rtu.Title = e.Title;
        }
        public void Apply(RtuRemoved e)
        {
            var rtu = Tree.GetById(e.Id);
            Tree.Remove(rtu);
        }
        #endregion

        #region Trace
        public void Apply(TraceAdded e)
        {
            var path = "pack://application:,,,/Resources/LeftPanel/Monitoring_Grey.png";
            var imageSource = new BitmapImage(new Uri(path));

            var trace = new Leaf()
            {
                Id = e.Id, LeafType = LeafType.Trace, Title = e.Title, Color = Brushes.Blue,
                Pic1 = imageSource,
                Pic2 = imageSource
            };
            var rtu = Tree.GetById(e.RtuId);
            rtu.Children.Add(trace);
            rtu.IsExpanded = true;
        }

        public void Apply(TraceAttached e)
        {
            var trace = Tree.GetById(e.TraceId);
            trace.Title = $"port:{e.Port} - {trace.Title}";
            trace.Color = Brushes.Black;
            trace.PortNumber = e.Port;
        }

        public void Apply(TraceDetached e)
        {
            var trace = Tree.GetById(e.TraceId);
            trace.Color = Brushes.Blue;
        }

        public void Apply(BaseRefAssigned e)
        {
        }
        #endregion
    }
}
