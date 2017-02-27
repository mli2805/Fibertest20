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
        public override string Name => Title;
        public PortLeaf(ReadModel readModel, IWindowManager windowManager, Bus bus, int portNumber) : base(readModel, windowManager, bus)
        {
            PortNumber = portNumber;
            Title = string.Format(Resources.SID_Port_N, PortNumber);
            Color = Brushes.Blue;
        }

        protected override List<MenuItemVm> GetMenuItems()
        {
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
                Command = new ContextMenuAction(AttachBopAction, CanSomeAction),
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

        private void AttachFromListAction(object param)
        {
            var vm = new TraceToAttachViewModel(Parent.Id, PortNumber, ReadModel, Bus);
            WindowManager.ShowDialog(vm);
        }
        private void AttachBopAction(object param) { }
    }
}