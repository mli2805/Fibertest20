using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class RtuLeaf : Leaf, IPortOwner
    {
        private readonly RtuLeafContextMenuProvider _rtuLeafContextMenuProvider;

        #region Pictograms
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

        private RtuPartState _bopState;
        public RtuPartState BopState
        {
            get => _bopState;
            set
            {
                if (value == _bopState) return;
                _bopState = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(BopPictogram));
            }
        }

        private RtuPartState _mainChannelState;
        public RtuPartState MainChannelState
        {
            get => _mainChannelState;
            set
            {
                if (value == _mainChannelState) return;
                _mainChannelState = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(MainChannelPictogram));
            }
        }

        private RtuPartState _reserveChannelState;
        public RtuPartState ReserveChannelState
        {
            get => _reserveChannelState;
            set
            {
                if (value == _reserveChannelState) return;
                _reserveChannelState = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(ReserveChannelPictogram));
            }
        }

        public bool IsAvailable => MainChannelState == RtuPartState.Ok ||
                                   ReserveChannelState == RtuPartState.Ok;

        public string MonitoringPictogram => GetPathToPictogram();
        public string BopPictogram => BopState.GetPathToPictogram();
        public string MainChannelPictogram => MainChannelState.GetPathToPictogram();
        public string ReserveChannelPictogram => ReserveChannelState.GetPathToPictogram();
        #endregion

        public int OwnPortCount { get; set; }
        public int FullPortCount { get; set; }
        public string Serial { get; set; }
        public NetAddress OtauNetAddress { get; set; }
        public override string Name => Title;
        public TreeOfAcceptableMeasParams TreeOfAcceptableMeasParams { get; set; }

        public ChildrenImpresario ChildrenImpresario { get; }
        public bool HasAttachedTraces =>  ChildrenImpresario.Children.Any(l => l is TraceLeaf && ((TraceLeaf) l).PortNumber > 0) 
                   || ChildrenImpresario.Children.Any(c => c is OtauLeaf && ((OtauLeaf) c).HasAttachedTraces);

        public int TraceCount => ChildrenImpresario.Children.Count(c => c is TraceLeaf) +
                ChildrenImpresario.Children.Where(c => c is OtauLeaf).Sum(otauLeaf => ((OtauLeaf)otauLeaf).TraceCount);

        public IPortOwner GetPortOwner(NetAddress netAddress)
        {
            if (OtauNetAddress.Equals(netAddress)) return this;
            return ChildrenImpresario.Children.Select(child => child as OtauLeaf).
                FirstOrDefault(otau => otau?.OtauNetAddress.Equals(netAddress) == true);
        }

        public RtuLeaf(RtuLeafContextMenuProvider rtuLeafContextMenuProvider, FreePorts view)
        {
            _rtuLeafContextMenuProvider = rtuLeafContextMenuProvider;
            ChildrenImpresario = new ChildrenImpresario(view);

            Title = Resources.SID_noname_RTU;
            Color = Brushes.DarkGray;
            MonitoringState = MonitoringState.Unknown;
            IsExpanded = true;
        }
        protected override List<MenuItemVm> GetMenuItems()
        {
            return _rtuLeafContextMenuProvider.GetMenu(this);
        }

        private string GetPathToPictogram()
        {
            switch (MonitoringState)
            {
                case MonitoringState.Unknown:
                    return @"pack://application:,,,/Resources/LeftPanel/EmptySquare.png";
                case MonitoringState.Off:
                    return @"pack://application:,,,/Resources/LeftPanel/GreySquare.png";
                case MonitoringState.On:
                    return @"pack://application:,,,/Resources/LeftPanel/BlueSquare.png";
                default:
                    return @"pack://application:,,,/Resources/LeftPanel/EmptySquare.png";
            }
        }
    }
}