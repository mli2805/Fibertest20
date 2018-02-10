using System.Windows;
using System.Windows.Input;
using GMap.NET.MapProviders;

namespace Iit.Fibertest.Client
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
            MainMap.OnTraceDefiningCancelled += MainMap_OnTraceDefiningCancelled;
        }

        private void MainMap_OnTraceDefiningCancelled()
        {
            SetBanner("");
        }

        private void ConfigureMap()
        {
            MainMap.MapProvider = GMapProviders.OpenStreetMap;
        }

        private void MapUserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null)
                return;
            var graph = (GraphReadModel)e.NewValue;
            graph.CurrentMousePosition = MainMap.Position;

            graph.Nodes.CollectionChanged += NodesCollectionChanged;
            graph.Fibers.CollectionChanged += FibersCollectionChanged;

            graph.PropertyChanged += Graph_PropertyChanged;

            MainMap.Zoom = graph.Zoom;
            MainMap.Position = graph.ToCenter;

            ApplyAddedNodes(graph.Nodes);
            ApplyAddedFibers(graph.Fibers);
        }

        private void Graph_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ToCenter")
                MainMap.Position = GraphReadModel.ToCenter;
        }

        void MainMap_MouseMove(object sender, MouseEventArgs e)
        {
            var p = e.GetPosition(MainMap);
            GraphReadModel.CurrentMousePosition =
                MainMap.FromLocalToLatLng((int)p.X, (int)p.Y);
            GraphReadModel.CenterForIni = MainMap.Position;
        }

        void MainMap_MouseEnter(object sender, MouseEventArgs e)
        {
            MainMap.Focus();
        }

        public void SetBanner(string message)
        {
            GraphReadModel.CommonStatusBarViewModel.StatusBarMessage2 = message;
        }
    }
}