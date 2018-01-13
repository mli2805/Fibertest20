using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class TraceLeaf : Leaf, IPortNumber
    {
        private int _portNumber;
        public int PortNumber
        {
            get => _portNumber;
            set
            {
                if (value == _portNumber) return;
                _portNumber = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(IconsVisibility));
                NotifyOfPropertyChange(nameof(LeftMargin));
                NotifyOfPropertyChange(nameof(Name));
            }
        }

        public int LeftMargin => PortNumber < 1 ? 78 : Parent is RtuLeaf ? 53 : 74;
        public Visibility IconsVisibility => PortNumber > 0 ? Visibility.Visible : Visibility.Hidden;

        public override string Name
        {
            get
            {
                return PortNumber < 1
                  ? Title
                  : string.Format(Resources.SID_Port_trace, PortNumber, Title);
            }
            set { }
        }

        public BaseRefsSet BaseRefsSet { get; set; } = new BaseRefsSet();

        private MonitoringState _traceMonitoringState;
        public MonitoringState TraceMonitoringState
        {
            get => _traceMonitoringState;
            set
            {
                if (value == _traceMonitoringState) return;
                _traceMonitoringState = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(MonitoringPictogram));
            }
        }

        private bool _isInMonitoringCycle;
        public bool IsInMonitoringCycle
        {
            get => _isInMonitoringCycle;
            set
            {
                if (value == _isInMonitoringCycle) return;
                _isInMonitoringCycle = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(MonitoringPictogram));
            }
        }


        private FiberState _traceState;
        public FiberState TraceState
        {
            get => _traceState;
            set
            {
                if (value == _traceState) return;
                _traceState = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(TraceStatePictogram));
            }
        }

        public ImageSource MonitoringPictogram => GetPictogram();
        public ImageSource TraceStatePictogram => TraceState.GetPictogram();

        private readonly TraceLeafContextMenuProvider _contextMenuProvider;

        public TraceLeaf(IPortOwner parent, TraceLeafContextMenuProvider contextMenuProvider)
        {
            Parent = (Leaf)parent;
            _contextMenuProvider = contextMenuProvider;
        }

        protected override List<MenuItemVm> GetMenuItems()
        {
            return _contextMenuProvider.GetMenu(this);
        }

        private ImageSource GetPictogram()
        {
            return IsInMonitoringCycle
                ? TraceMonitoringState == MonitoringState.On
                    ? new BitmapImage(new Uri("pack://application:,,,/Resources/LeftPanel/BlueSquare.png"))
                    : new BitmapImage(new Uri("pack://application:,,,/Resources/LeftPanel/GreySquare.png"))
                : BaseRefsSet.HasEnoughBaseRefsToPerformMonitoring
                    ? new BitmapImage(new Uri("pack://application:,,,/Resources/LeftPanel/GreyHalfSquare.png"))
                    : new BitmapImage(new Uri("pack://application:,,,/Resources/LeftPanel/EmptySquare.png"));
        }
    }
}



