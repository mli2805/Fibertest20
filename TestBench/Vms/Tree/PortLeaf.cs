using System.Collections.Generic;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.TestBench
{
    public static class PortExtensions
    {
        public static void MeasurementClientAction(object param) { }
        public static void MeasurementRftsReflectAction(object param) { }
    }

    public class PortLeaf : Leaf
    {
        public int PortNumber { get; set; }
        public PortLeaf(ReadModel readModel, IWindowManager windowManager, Bus bus) : base(readModel, windowManager, bus)
        {
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
                Header = Resources.SID_Attach_BOP,
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

        private void AttachFromListAction(object param) { }
        private void AttachBopAction(object param) { }
    }
}