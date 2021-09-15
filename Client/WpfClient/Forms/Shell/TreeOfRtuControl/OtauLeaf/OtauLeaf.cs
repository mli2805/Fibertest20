using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public class OtauLeaf : Leaf, IPortOwner
    {
        private readonly IWcfServiceCommonC2D _c2RWcfManager;
        private readonly Model _readModel;
        private readonly CurrentUser _currentUser;
        private RtuPartState _otauState;
        public int OwnPortCount { get; set; }
        public int MasterPort { get; set; }
        public NetAddress OtauNetAddress { get; set; }
        public string Serial { get; set; }

        public RtuPartState OtauState
        {
            get => _otauState;
            set
            {
                if (value == _otauState) return;
                _otauState = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(OtauStatePictogram));
            }
        }

        public string OtauStatePictogram => OtauState.GetPathToPictogram();

        public bool HasAttachedTraces =>
            ChildrenImpresario.Children.Any(l => l is TraceLeaf && ((TraceLeaf)l).PortNumber > 0);

        public override string Name => string.Format(Resources.SID_Port_trace, MasterPort, Title);

        public ChildrenImpresario ChildrenImpresario { get; }
        public int TraceCount => ChildrenImpresario.Children.Count(c => c is TraceLeaf);

        public OtauLeaf(IWcfServiceCommonC2D c2RWcfManager, Model readModel,
            CurrentUser currentUser, FreePorts freePorts)
        {
            _c2RWcfManager = c2RWcfManager;
            _readModel = readModel;
            _currentUser = currentUser;
            ChildrenImpresario = new ChildrenImpresario(freePorts);
        }

        protected override List<MenuItemVm> GetMenuItems()
        {
            IsSelected = true;

            return new List<MenuItemVm>
            {
                new MenuItemVm()
                {
                    Header = Resources.SID_Remove,
                    Command = new ContextMenuAction(OtauRemoveAction, CanOtauRemoveAction),
                    CommandParameter = this
                }
            };
        }

        public async void OtauRemoveAction(object param)
        {
            var rtu = _readModel.Rtus.First(r => r.Id == Parent.Id);

            var dto = new DetachOtauDto()
            {
                OtauId = Id, 
                RtuId = rtu.Id,
                RtuMaker = rtu.RtuMaker,
                OpticalPort = MasterPort, 
                NetAddress = (NetAddress)OtauNetAddress.Clone(),
            };
            using (new WaitCursor())
            {
                 await _c2RWcfManager.DetachOtauAsync(dto);
            }
        }

        private bool CanOtauRemoveAction(object param)
        {
            if (!(param is OtauLeaf otauLeaf))
                return false;

            var rtuLeaf = (RtuLeaf)otauLeaf.Parent;

            return _currentUser.Role <= Role.Root
                   && rtuLeaf.IsAvailable
                   && rtuLeaf.MonitoringState == MonitoringState.Off;
        }
    }
}