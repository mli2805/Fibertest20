using System.Collections.Generic;
using System.Windows.Media;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.TestBench
{
    public class PortLeaf : Leaf
    {
        public readonly int PortNumber;
        public readonly int ExtendedPortNumber;

        public override string Name
        {
            get
            {
                return Parent is OtauLeaf ?
                    string.Format(Resources.SID_Port_N_on_otau, ExtendedPortNumber, ((OtauLeaf)Parent).MasterPort, PortNumber) :
                    string.Format(Resources.SID_Port_N, PortNumber);
            }
            set { }
        }
        public int LeftMargin => Parent is OtauLeaf ? 65 : 85;

        public PortLeaf(ReadModel readModel, IWindowManager windowManager, Bus bus, Leaf parent, int portNumber)
            : base(readModel, windowManager, bus)
        {
            PortNumber = portNumber;
            Parent = parent;
            var otauLeaf = Parent as OtauLeaf;
            ExtendedPortNumber = otauLeaf != null ? otauLeaf.FirstPortNumber + PortNumber - 1 : PortNumber;
            Color = Brushes.Blue;
        }

        protected override List<MenuItemVm> GetMenuItems()
        {
            IsSelected = true;

            var menu = new List<MenuItemVm>();
            menu.AddRange(GetFreePortMenuItems());
            menu.AddRange(GetAnyPortMenuItems());
            return menu;
        }
        private IEnumerable<MenuItemVm> GetFreePortMenuItems()
        {
            yield return new MenuItemVm()
            {
                Header = Resources.SID_Attach_from_list,
                Command = new ContextMenuAction(AttachFromListAction, CanSomeAction),
                CommandParameter = this,
            };

            yield return new MenuItemVm()
            {
                Header = Resources.SID_Attach_optical_switch,
                Command = new ContextMenuAction(AttachOtauAction, CanAttachOtauAction),
                CommandParameter = this,
            };
        }

        private IEnumerable<MenuItemVm> GetAnyPortMenuItems()
        {
            yield return null;

            yield return new MenuItemVm()
            {
                Header = Resources.SID_Measurement__Client_,
                Command = new ContextMenuAction(PortExtensions.MeasurementClientAction, CanSomeAction),
                CommandParameter = this,
            };

            yield return new MenuItemVm()
            {
                Header = Resources.SID_Measurement__RFTS_Reflect_,
                Command = new ContextMenuAction(PortExtensions.MeasurementRftsReflectAction, CanSomeAction),
                CommandParameter = this,
            };
        }

        public void AttachFromListAction(object param)
        {
            var rtuId = Parent is RtuLeaf ? Parent.Id : Parent.Parent.Id;
            var vm = new TraceToAttachViewModel(rtuId, ExtendedPortNumber, ReadModel, Bus);
            WindowManager.ShowDialog(vm);
        }

        public void AttachOtauAction(object param)
        {
            var vm = new OtauToAttachViewModel(Parent.Id, PortNumber, ReadModel, Bus, WindowManager);
            WindowManager.ShowDialog(vm);
        }

        private bool CanAttachOtauAction(object param)
        {
            if (Parent is OtauLeaf)
                return false;
            var rtuLeaf = (RtuLeaf)Parent;
            return !rtuLeaf.HasAttachedTraces;
        }
    }
}