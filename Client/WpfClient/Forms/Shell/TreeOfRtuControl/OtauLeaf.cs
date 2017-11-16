using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class OtauLeaf : Leaf, IPortOwner
    {
        public RtuPartState State { get; set; }
        public int OwnPortCount { get; set; }
        public int FirstPortNumber { get; set; }
        public int MasterPort { get; set; }
        public NetAddress OtauNetAddress { get; set; }
        public RtuPartState OtauState { get; set; }
        public ImageSource OtauStatePictogram => OtauState.GetPictogram();

        public override string Name
        {
            get { return string.Format(Resources.SID_Port_trace, MasterPort, Title); }
            set { }
        }
        public ChildrenImpresario ChildrenImpresario { get; }
        public int TraceCount => ChildrenImpresario.Children.Count(c => c is TraceLeaf);

        public OtauLeaf(ReadModel readModel, IWindowManager windowManager, IWcfServiceForClient c2DWcfManager, PostOffice postOffice, FreePorts freePorts) 
            : base(readModel, windowManager, c2DWcfManager, postOffice)
        {
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

        public void OtauRemoveAction(object param)
        {
            C2DWcfManager.SendCommandAsObj(new DetachOtau() { Id = Id, RtuId = Parent.Id });
        }

        private bool CanOtauRemoveAction(object param)
        {
            return ChildrenImpresario.Children.All(c => c is PortLeaf);
        }

    }
}