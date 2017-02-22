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
            graph.CurrentMousePosition = MainMap.Position.ToStringInDegrees();

            graph.Nodes.CollectionChanged += NodesCollectionChanged;
            graph.Fibers.CollectionChanged += FibersCollectionChanged;
        }


        void MainMap_MouseMove(object sender, MouseEventArgs e)
        {
            var p = e.GetPosition(MainMap);
            GraphVm.CurrentMousePosition = 
                MainMap.FromLocalToLatLng((int)p.X, (int)p.Y).ToStringInDegrees();
        }

        void MainMap_MouseEnter(object sender, MouseEventArgs e)
        {
            MainMap.Focus();
        }
    }
}