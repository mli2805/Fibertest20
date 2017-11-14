using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class TraceLeaf : Leaf
    {
        private int _portNumber;
        public int PortNumber
        {
            get { return _portNumber; }
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
        public int ExtendedPortNumber => Parent is OtauLeaf ? ((OtauLeaf)Parent).FirstPortNumber + PortNumber - 1 : PortNumber;

        public int LeftMargin => PortNumber < 1 ? 78 : Parent is RtuLeaf ? 53 : 74;
        public Visibility IconsVisibility => PortNumber > 0 ? Visibility.Visible : Visibility.Hidden;

        public override string Name
        {
            get { return PortNumber < 1 ? 
                            Title : 
                            Parent is OtauLeaf ?
                                string.Format(Resources.SID_Port_trace_on_otau, PortNumber, ExtendedPortNumber, Title) :
                                string.Format(Resources.SID_Port_trace, PortNumber, Title) ; }
            set { }
        }

        private MonitoringState _monitoringState;
        public MonitoringState MonitoringState
        {
            get { return _monitoringState; }
            set
            {
                if (value == _monitoringState) return;
                _monitoringState = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(MonitoringPictogram));
            }
        }

      
        private FiberState _traceState;
        public FiberState TraceState
        {
            get { return _traceState; }
            set
            {
                if (value == _traceState) return;
                _traceState = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(TraceStatePictogram));
            }
        }

        public ImageSource MonitoringPictogram => MonitoringState.GetPictogram();
        public ImageSource TraceStatePictogram => TraceState.GetPictogram();

        private readonly TraceLeafContextMenuProvider _contextMenuProvider;

        public TraceLeaf(ReadModel readModel, IWindowManager windowManager, IWcfServiceForClient c2DWcfManager, 
            PostOffice postOffice, IPortOwner parent, TraceLeafContextMenuProvider contextMenuProvider) 
            : base(readModel, windowManager, c2DWcfManager, postOffice)
        {
            Parent = (Leaf)parent;
            _contextMenuProvider = contextMenuProvider;
        }

        protected override List<MenuItemVm> GetMenuItems()
        {
            return _contextMenuProvider.GetMenu(this);
        }

    }
}



