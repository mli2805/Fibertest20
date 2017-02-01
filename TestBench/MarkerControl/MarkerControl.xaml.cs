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

namespace Iit.Fibertest.TestBench
{
    /// <summary>
        /// Interaction logic for MarkerControl.xaml
        /// </summary>
        public partial class MarkerControl : INotifyPropertyChanged
    {
        private Popup _popup;
        private Label _label;
        private readonly GMapMarker _marker;
        private readonly EquipmentType _type;
        private readonly string _title;
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

        public MarkerControl(Map mainMap, GMapMarker marker, EquipmentType type, string title)
        {
            InitializeComponent();
            _mainMap = mainMap;
            _marker = marker;
            _type = type;
            _title = title;

            Subscribe();
            InitializePopup();
        }

        private void Subscribe()
        {
            Unloaded += MarkerControl_Unloaded;
            Loaded += MarkerControl_Loaded;
            SizeChanged += MarkerControl_SizeChanged;
            MouseEnter += MarkerControl_MouseEnter;
            MouseLeave += MarkerControl_MouseLeave;

            PreviewMouseMove += MarkerControl_PreviewMouseMove;
            PreviewMouseLeftButtonUp += MarkerControl_PreviewMouseLeftButtonUp;
            PreviewMouseLeftButtonDown += MarkerControl_PreviewMouseLeftButtonDown;
            PreviewMouseRightButtonUp += MarkerControl_PreviewMouseRightButtonUp;
        }

        private void InitializePopup()
        {
            _label = new Label { Background = Brushes.White, Foreground = Brushes.Black, FontSize = 14, Content = _title, };
            _popup = new Popup { Placement = PlacementMode.Mouse, Child = _label, };
        }

        private void AssignBitmapImage(Image pictogram)
        {
            pictogram.Width = _type == EquipmentType.Rtu ? 40 : 8;
            pictogram.Height = _type == EquipmentType.Rtu ? 28 : 8;

            pictogram.Source = Utils.GetPictogramBitmapImage(_type, FiberState.Ok);
        }

        void MarkerControl_Loaded(object sender, RoutedEventArgs e)
        {
            AssignBitmapImage(Icon);
            if (Icon.Source.CanFreeze)
                Icon.Source.Freeze();
        }

        void MarkerControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Unloaded -= MarkerControl_Unloaded;
            Loaded -= MarkerControl_Loaded;
            SizeChanged -= MarkerControl_SizeChanged;
            MouseEnter -= MarkerControl_MouseEnter;
            MouseLeave -= MarkerControl_MouseLeave;

            PreviewMouseMove -= MarkerControl_PreviewMouseMove;
            PreviewMouseLeftButtonUp -= MarkerControl_PreviewMouseLeftButtonUp;
            PreviewMouseLeftButtonDown -= MarkerControl_PreviewMouseLeftButtonDown;
            PreviewMouseRightButtonUp -= MarkerControl_PreviewMouseRightButtonUp;

            _marker.Shape = null;
            Icon.Source = null;
            Icon = null;
            _popup = null;
            _label = null;
        }


        public bool IsMenuItemRemoveNodeEnabled { get; set; }
        private void MarkerContextMenuOnClick(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuItem;
            if (item == null) return;
            var code = int.Parse((string)item.Tag);

            switch (code)
            {
                case 4:
                    Command = new RemoveNode() { Id = _marker.Id };
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
