using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using Iit.Fibertest.UtilsLib;
using JetBrains.Annotations;

namespace Iit.Fibertest.Client
{
    /// <summary>
    /// Interaction logic for MapUserControl.xaml
    /// </summary>
    public partial class MapUserControl : INotifyPropertyChanged
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
            graph.MainMap = MainMap;
            graph.CurrentMousePosition = MainMap.Position;

            graph.Nodes.CollectionChanged += NodesCollectionChanged;
            graph.Fibers.CollectionChanged += FibersCollectionChanged;

            graph.PropertyChanged += Graph_PropertyChanged;

            ApplyAddedNodes(graph.Nodes);
            ApplyAddedFibers(graph.Fibers);
        }

        private void Graph_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(GraphReadModel.SelectedGraphVisibilityItem))
                ChangeVisibility(GraphReadModel.SelectedGraphVisibilityItem.Level);
        }

        public void ChangeVisibility(GraphVisibilityLevel selectedLevel)
        {
            foreach (var marker in MainMap.Markers)
            {
                if (marker is GMapRoute gMapRoute)
                {
                    gMapRoute.Shape.Visibility = selectedLevel >= GraphVisibilityLevel.RtuAndTraces
                        ? Visibility.Visible
                        : Visibility.Hidden;
                }
                else if (marker is GMapMarker gMapMarker) // GMapMarker - node
                {
                    gMapMarker.Shape.Visibility =
                        selectedLevel >= ((MarkerControl)gMapMarker.Shape).EqType.GetEnabledVisibilityLevel()
                        ? Visibility.Visible
                        : Visibility.Hidden;
                }
            }
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

        public void SetBanner(string message)
        {
            GraphReadModel.CommonStatusBarViewModel.StatusBarMessage2 = message;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private void UserControl_LostFocus(object sender, RoutedEventArgs e)
        {
            GraphReadModel.IniFile.Write(IniSection.Map, IniKey.Zoom, MainMap.Zoom);
            GraphReadModel.IniFile.Write(IniSection.Map, IniKey.CenterLatitude, MainMap.Position.Lat);
            GraphReadModel.IniFile.Write(IniSection.Map, IniKey.CenterLongitude, MainMap.Position.Lng);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            MainMap.Zoom = GraphReadModel.IniFile.Read(IniSection.Map, IniKey.Zoom, 7);
            var lat = GraphReadModel.IniFile.Read(IniSection.Map, IniKey.CenterLatitude, 53.856);
            var lng = GraphReadModel.IniFile.Read(IniSection.Map, IniKey.CenterLongitude, 27.49);
            MainMap.Position = new PointLatLng(lat, lng);
        }

        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.L && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                SetBanner(MainMap.IsInDistanceMeasurementMode ? "" : "Distance measurement mode");

                MainMap.IsInDistanceMeasurementMode = !MainMap.IsInDistanceMeasurementMode;
                if (MainMap.IsInDistanceMeasurementMode)
                {
                    MainMap.DistanceMarkers = new List<GMapMarker>();
                    MainMap.StartNode = null;
                }
            }
        }
    }
}