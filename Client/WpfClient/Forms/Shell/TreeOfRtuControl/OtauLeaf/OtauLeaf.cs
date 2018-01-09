using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class OtauLeaf : Leaf, IPortOwner
    {
        public IWcfServiceForClient C2DWcfManager { get; }
        private readonly CurrentUser _currentUser;
        public int OwnPortCount { get; set; }
        public int MasterPort { get; set; }
        public NetAddress OtauNetAddress { get; set; }
        public RtuPartState OtauState { get; set; }
        public ImageSource OtauStatePictogram => OtauState.GetPictogram();

        public bool HasAttachedTraces =>
            ChildrenImpresario.Children.Any(l => l is TraceLeaf && ((TraceLeaf) l).PortNumber > 0);

        public override string Name => string.Format(Resources.SID_Port_trace, MasterPort, Title);

        public ChildrenImpresario ChildrenImpresario { get; }
        public int TraceCount => ChildrenImpresario.Children.Count(c => c is TraceLeaf);

        public OtauLeaf(IWcfServiceForClient c2DWcfManager,
            CurrentUser currentUser, FreePorts freePorts)
        {
            C2DWcfManager = c2DWcfManager;
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
            var dto = new DetachOtauDto() {OtauId = Id, RtuId = Parent.Id, OpticalPort = MasterPort};
            using (new WaitCursor())
            {
                var result = await C2DWcfManager.DetachOtauAsync(dto);
                if (result.IsDetached)
                {
                    RemoveOtauFromGraph();
                }
            }
        }

        public async void RemoveOtauFromGraph()
        {
            await C2DWcfManager.SendCommandAsObj(new DetachOtau() {Id = Id, RtuId = Parent.Id});
        }

        private bool CanOtauRemoveAction(object param)
        {
            return _currentUser.Role <= Role.Root
                   && ChildrenImpresario.Children.All(c => c is PortLeaf);
        }
    }
}