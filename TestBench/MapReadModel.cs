using System;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace Iit.Fibertest.TestBench
{
    public class Vertex
    {
        public Guid Id { get; set; }
        public ImageSource Pictogram { get; set; }
    }

    public class Edge
    {
        public Guid Id { get; set; }

    }

    public class MapReadModel
    {
        public ObservableCollection<Vertex> Vertices { get; set; }
        public ObservableCollection<Edge> Edges { get; set; }
    }
}
