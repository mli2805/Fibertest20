using System.Collections.Generic;
using System.Windows.Media;
using Caliburn.Micro;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class PortLeaf : Leaf, IPortNumber
    {
        private readonly PortLeafContextMenuProvider _contextMenuProvider;
        public int PortNumber { get; set; }

        public override string Name
        {
            get { return string.Format(Resources.SID_Port_N, PortNumber); }
            set { }
        }
        public int LeftMargin => Parent is OtauLeaf ? 106 : 85;

        public PortLeaf(
            // Specified manually
            Leaf parent, int portNumber, 
            // Resolved with Autofac
            ReadModel readModel, IWindowManager windowManager,
            IWcfServiceForClient c2DWcfManager,
            PortLeafContextMenuProvider contextMenuProvider)
            : base(readModel, windowManager, c2DWcfManager)
        {
            PortNumber = portNumber;
            _contextMenuProvider = contextMenuProvider;
            Parent = parent;
            Color = Brushes.Black;
        }

        protected override List<MenuItemVm> GetMenuItems()
        {
            return _contextMenuProvider.GetMenu(this);
        }

    }
}