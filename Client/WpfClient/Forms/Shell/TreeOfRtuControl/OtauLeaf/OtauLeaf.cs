using System;
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
        public IWcfServiceForClient C2DWcfManager { get; }
        private readonly IWcfServiceForC2R _c2RWcfManager;
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

        public OtauLeaf(IWcfServiceForClient c2DWcfManager, IWcfServiceForC2R c2RWcfManager, 
            CurrentUser currentUser, FreePorts freePorts)
        {
            C2DWcfManager = c2DWcfManager;
            _c2RWcfManager = c2RWcfManager;
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
            if (!(param is OtauLeaf otauLeaf)) return;

            var dto = new DetachOtauDto() { OtauId = Id, RtuId = Parent.Id, OpticalPort = MasterPort };
            using (new WaitCursor())
            {
                var result = await _c2RWcfManager.DetachOtauAsync(dto);
                if (result.IsDetached)
                {
                    RemoveOtauFromGraph(otauLeaf);
                }
            }
        }

        private DetachOtau CreateCmd()
        {
            return new DetachOtau()
            {
                Id = Id,
                RtuId = Parent.Id,
                OtauIp = OtauNetAddress.Ip4Address,
                TcpPort = OtauNetAddress.Port,
                TracesOnOtau = new List<Guid>()
            };
        }

        public async void RemoveOtauFromGraph(OtauLeaf otauLeaf)
        {
            var cmd = CreateCmd();
            foreach (var child in otauLeaf.ChildrenImpresario.Children)
            {
                if (child is TraceLeaf traceLeaf)
                    cmd.TracesOnOtau.Add(traceLeaf.Id);
            }
            await C2DWcfManager.SendCommandAsObj(cmd);
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