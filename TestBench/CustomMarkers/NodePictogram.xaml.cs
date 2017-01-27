using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using GMap.NET.WindowsPresentation;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Commands;
using Iit.Fibertest.TestBench.Properties;

namespace Iit.Fibertest.TestBench.CustomMarkers
{
    /// <summary>
    /// Interaction logic for NodePictogram.xaml
    /// </summary>
    public partial class NodePictogram : INotifyPropertyChanged
    {
        private Popup _popup;
        private Label _label;
        private readonly GMapMarker _marker;
        private readonly EquipmentType _type;
        private readonly Map _mainMap;

        private object _command;
        public object Command
        {
            get { return _command; }
            set
            {
                if (Equals(value, _command)) return;
                _command = value;
                OnPropertyChanged();
            }
        }

        public NodePictogram(Map mainMap, GMapMarker marker, EquipmentType type, string title)
        {
            InitializeComponent();
            _mainMap = mainMap;
            _marker = marker;
            _type = type;

            _popup = new Popup();
            _label = new Label();

            Unloaded += NodePictogram_Unloaded;
            Loaded += NodePictogram_Loaded;
            SizeChanged += NodePictogram_SizeChanged;
            MouseEnter += NodePictogram_MouseEnter;
            MouseLeave += NodePictogram_MouseLeave;

            PreviewMouseMove += NodePictogram_PreviewMouseMove;
            PreviewMouseLeftButtonUp += NodePictogram_PreviewMouseLeftButtonUp;
            PreviewMouseLeftButtonDown += NodePictogram_PreviewMouseLeftButtonDown;
            PreviewMouseRightButtonUp += NodePictogram_PreviewMouseRightButtonUp;

            _popup.Placement = PlacementMode.Mouse;
            {
                _label.Background = Brushes.White;
                _label.Foreground = Brushes.Black;
                //                            _label.Opacity = 0.2;
                //                            _label.BorderBrush = Brushes.WhiteSmoke;
                //                            _label.BorderThickness = new Thickness(2);
                //                            _label.Padding = new Thickness(5);
                _label.FontSize = 14;
                _label.Content = title;
            }
            _popup.Child = _label;
        }

        private void NodePictogram_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            var contextMenu = FindResource("MarkerContextMenu") as ContextMenu;
            if (contextMenu == null) return;

            //TODO define which menu items are disabled
            contextMenu.IsOpen = true;
        }

        private void AssignBitmapImage(Image pictogram)
        {
            pictogram.Width = _type == EquipmentType.Rtu ? 40 : 8;
            pictogram.Height = _type == EquipmentType.Rtu ? 28 : 8;

            pictogram.Source = Utils.GetPictogramBitmapImage(_type, FiberState.Ok);
        }

        void NodePictogram_Loaded(object sender, RoutedEventArgs e)
        {
            AssignBitmapImage(Icon);
            if (Icon.Source.CanFreeze)
                Icon.Source.Freeze();
        }

        void NodePictogram_Unloaded(object sender, RoutedEventArgs e)
        {
            Unloaded -= NodePictogram_Unloaded;
            Loaded -= NodePictogram_Loaded;
            SizeChanged -= NodePictogram_SizeChanged;
            MouseEnter -= NodePictogram_MouseEnter;
            MouseLeave -= NodePictogram_MouseLeave;

            PreviewMouseMove -= NodePictogram_PreviewMouseMove;
            PreviewMouseLeftButtonUp -= NodePictogram_PreviewMouseLeftButtonUp;
            PreviewMouseLeftButtonDown -= NodePictogram_PreviewMouseLeftButtonDown;
            PreviewMouseRightButtonUp -= NodePictogram_PreviewMouseRightButtonUp;

            _marker.Shape = null;
            Icon.Source = null;
            Icon = null;
            _popup = null;
            _label = null;
        }

        // происходит при запуске приложения поэтому используется для инициализации offset
        // offset задает смещение пиктограммы относительно координат
        void NodePictogram_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _marker.Offset = new Point(-e.NewSize.Width / 2, -e.NewSize.Height / 2);
        }

        // при нажатом Ctrl левая кнопка таскает данный маркер
        private void NodePictogram_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && IsMouseCaptured && (Keyboard.Modifiers & ModifierKeys.Control) != 0)
            {
                DragMarkerWithItsFibers(e);
            }

            e.Handled = true;
        }

        private void DragMarkerWithItsFibers(MouseEventArgs e)
        {
            Point p = e.GetPosition(_mainMap);
            _marker.Position = _mainMap.FromLocalToLatLng((int) (p.X), (int) (p.Y));

            foreach (var route in _mainMap.Markers.Where(m => m is GMapRoute))
            {
                var r = (GMapRoute) route;
                if (r.LeftId != _marker.Id && r.RightId != _marker.Id)
                    continue;
                if (r.LeftId == _marker.Id)
                    r.Points[0] = _marker.Position;
                if (r.RightId == _marker.Id)
                    r.Points[1] = _marker.Position;
                r.RegenerateShape(_mainMap);
            }
        }

        void NodePictogram_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsMouseCaptured && (Keyboard.Modifiers & ModifierKeys.Control) != 0)
            {
                Mouse.Capture(this);
            }
            e.Handled = true;
        }

        void NodePictogram_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (IsMouseCaptured)
            {
                Mouse.Capture(null);
                Command = new MoveNode() { Id = _marker.Id, Latitude = _marker.Position.Lat, Longitude = _marker.Position.Lng };
            }

            if (_mainMap.IsInFiberCreationMode)
            {
                EndFiberCreation();
            }
            e.Handled = true;
        }

        private void EndFiberCreation()
        {
            _mainMap.IsInFiberCreationMode = false;
            // it was a temporary fiber for creation purposes only
            _mainMap.Markers.Remove(_mainMap.Markers.Single(m => m.Id == _mainMap.FiberUnderCreation));

            if (!_mainMap.IsFiberWithNodes)
                Command = new AddFiber() {Node1 = _mainMap.StartNode.Id, Node2 = _marker.Id};
            else
                Command = new AddFiberWithNodes() { Node1 = _mainMap.StartNode.Id, Node2 = _marker.Id,  };

            _mainMap.FiberUnderCreation = Guid.Empty;
            Cursor = Cursors.Arrow;
        }


        void NodePictogram_MouseLeave(object sender, MouseEventArgs e)
        {
            _marker.ZIndex -= 10000;
            Cursor = Cursors.Arrow;
            _popup.IsOpen = false;
        }

        void NodePictogram_MouseEnter(object sender, MouseEventArgs e)
        {
            _marker.ZIndex += 10000;
            Cursor = Cursors.Hand;

            _popup.IsOpen = true;
        }

        public int FiberCreateWithNodesCount { get; set; }
        public EquipmentType FiberCreateWithNodesType { get; set; }

        public bool IsRemoveNodeEnabled { get; set; }
        private void MarkerContextMenuOnClick(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuItem;
            if (item == null) return;
            var code = int.Parse((string)item.Tag);

            switch (code)
            {
                case 4:
                    Command = new RemoveNode() {Id = _marker.Id};
                    return;
                case 5:
                    _mainMap.IsFiberWithNodes = false;
                    _mainMap.IsInFiberCreationMode = true;
                    _mainMap.StartNode = _marker;
                    Cursor = Cursors.Pen;
                    return;
                case 6:
                    _mainMap.IsFiberWithNodes = true;
                    _mainMap.IsInFiberCreationMode = true;
                    _mainMap.StartNode = _marker;
                    Cursor = Cursors.Pen;
                    return;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
