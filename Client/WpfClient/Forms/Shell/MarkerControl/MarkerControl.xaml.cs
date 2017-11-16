using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using GMap.NET.WindowsPresentation;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    /// <summary>
    /// Interaction logic for MarkerControl.xaml
    /// </summary>
    public partial class MarkerControl 
    {
        private Popup _popup;
        private Label _label;
        private readonly GMapMarker _marker;

        public EquipmentType Type
        {
            get { return _type; }
            set
            {
                _type = value;
                AssignBitmapImageTo(Icon);
            }
        }

        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                InitializePopup();
            }
        }

        private readonly Map _mainMap;
        private readonly MapUserControl _owner;
        private EquipmentType _type;
        private string _title;

        public new ContextMenu ContextMenu { get; set; }

        public MarkerControl(MapUserControl owner, GMapMarker marker, EquipmentType type, string title)
        {
            InitializeComponent();
            _owner = owner;
            _mainMap = owner.MainMap;
            _marker = marker;
            Type = type;
            Title = title;

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
            _label = new Label { Background = Brushes.White, Foreground = Brushes.Black, FontSize = 14, Content = Title, };
            _popup = new Popup { Placement = PlacementMode.Mouse, Child = _label, };
        }

        private void AssignBitmapImageTo(Image destination)
        {
            destination.Width = Type == EquipmentType.Rtu ? 40 : 8;
            destination.Height = Type == EquipmentType.Rtu ? 28 : 8;
            
            destination.Source = Interpreter.GetPictogramBitmapImage(Type, FiberState.Ok);
            destination.ContextMenu = ContextMenu;
        }

        void MarkerControl_Loaded(object sender, RoutedEventArgs e)
        {
            AssignBitmapImageTo(Icon);
            if (Icon.Source.CanFreeze)
                Icon.Source.Freeze();
            Icon.Visibility = Visibility.Visible;
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
    }
}
