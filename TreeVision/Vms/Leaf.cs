using System;
using System.Collections.ObjectModel;
using System.Windows.Media;
using Iit.Fibertest.Graph;

namespace TreeVision
{
    public enum LeafType
    {
        DataCenter,
        Rtu,
        Bop,
        Trace
    }
    public class Leaf
    {
        public string Title { get; set; }
        public ObservableCollection<Leaf> Children { get; set; } = new ObservableCollection<Leaf>();

        public Guid Id { get; set; }
        public LeafType LeafType { get; set; }

        public FiberState State { get; set; }
        public Brush Color { get; set; }
    }
}
