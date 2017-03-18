using System.Windows;
using System.Windows.Input;
using GMap.NET;
using GMap.NET.MapProviders;

namespace Iit.Fibertest.TestBench
{
    /// <summary>
        /// Interaction logic for MapUserControl.xaml
        /// </summary>
        public partial class MapUserControl
    {
        public GraphReadModel GraphReadModel => (GraphReadModel)DataContext;

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
            var graph = (GraphReadModel)e.NewValue;
            graph.CurrentMousePosition = MainMap.Position;

            graph.Nodes.CollectionChanged += NodesCollectionChanged;
            graph.Fibers.CollectionChanged += FibersCollectionChanged;

           ApplyAddedNodes(graph.Nodes);
           ApplyAddedFibers(graph.Fibers);
        }


        void MainMap_MouseMove(object sender, MouseEventArgs e)
        {
            var p = e.GetPosition(MainMap);
            GraphReadModel.CurrentMousePosition = 
                MainMap.FromLocalToLatLng((int)p.X, (int)p.Y);
        }

        void MainMap_MouseEnter(object sender, MouseEventArgs e)
        {
            MainMap.Focus();
        }
    }
}