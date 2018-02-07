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
        public readonly GMapMarker GMapMarker;

        public EquipmentType Type
        {
            get => _type;
            set
            {
                _type = value;
                AssignBitmapImageTo(Icon);
            }
        }

        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                InitializePopup();
            }
        }

        public readonly Map MainMap;
        public readonly MapUserControl Owner;
        private EquipmentType _type;
        private string _title;

        public new ContextMenu ContextMenu { get; set; }

        public MarkerControl(MapUserControl owner, GMapMarker gMapMarker, EquipmentType type, string title)
        {
            InitializeComponent();
            Owner = owner;
            MainMap = owner.MainMap;
            GMapMarker = gMapMarker;
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
            
            destination.Source = EquipmentTypeExt.GetPictogramBitmapImage(Type, FiberState.Ok);
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

            GMapMarker.Shape = null;
            Icon.Source = null;
            Icon = null;
            _popup = null;
            _label = null;
        }

        private void OpenNodeContextMenu()
        {
            var actions = new NodeVmActions();
            var actionsC = new CommonVmActions();
            var permissions = new NodeVmPermissions(new CurrentUser(){Role = Role.Root});
            var menuProvider = new NodeVmContextMenuProvider(actions, actionsC, permissions);
            ContextMenu = menuProvider.GetNodeContextMenu(this);
            ContextMenu.IsOpen = true;
        }
        private void OpenRtuContextMenu()
        {
            var actions = new RtuVmActions();
            var actionsC = new CommonVmActions();
            var permissions = new RtuVmPermissions(new CurrentUser(){Role = Role.Root});
            var menuProvider = new RtuVmContextMenuProvider(actions, actionsC, permissions);
            ContextMenu = menuProvider.GetRtuContextMenu(this);
            ContextMenu.IsOpen = true;
        }

    }
}
