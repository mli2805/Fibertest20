using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using GMap.NET.WindowsPresentation;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.TestBench
{
    /// <summary>
    /// Interaction logic for MarkerControl.xaml
    /// </summary>
    public partial class MarkerControl 
    {
        private Popup _popup;
        private Label _label;
        private readonly GMapMarker _marker;
        private readonly EquipmentType _type;
        private readonly string _title;
        private readonly Map _mainMap;
        private readonly MapUserControl _owner;

        public new ContextMenu ContextMenu { get; set; }

        public MarkerControl(MapUserControl owner, GMapMarker marker, EquipmentType type, string title)
        {
            InitializeComponent();
            _owner = owner;
            _mainMap = owner.MainMap;
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
            pictogram.ContextMenu = ContextMenu;
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
    }
}
