using System.Collections.Generic;
using System.Windows.Media;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.TestBench
{
    public class RtuLeaf : Leaf
    {
        public MonitoringState MonitoringState { get; set; }
        public RtuPartState BopState { get; set; }
        public RtuPartState MainChannelState { get; set; }
        public RtuPartState ReserveChannelState { get; set; }

        public ImageSource MonitoringPictogram => MonitoringState.GetPictogram();
        public ImageSource BopPictogram => BopState.GetPictogram();
        public ImageSource MainChannelPictogram => MainChannelState.GetPictogram();
        public ImageSource ReserveChannelPictogram => ReserveChannelState.GetPictogram();

        public int PortCount { get; set; }

        protected override List<MenuItemVm> GetMenuItems()
        {
            var menu = new List<MenuItemVm>();

            menu.Add(new MenuItemVm()
            {
                Header = Resources.SID_Information,
                Command = new ContextMenuAction(RtuInformationAction, CanSomeAction),
                CommandParameter = this
            });

            menu.Add(new MenuItemVm()
            {
                Header = Resources.SID_Show_RTU,
                Command = new ContextMenuAction(ShowRtuAction, CanSomeAction),
                CommandParameter = this
            });

            menu.Add(new MenuItemVm()
            {
                Header = Resources.SID_Settings,
                Command = new ContextMenuAction(RtuSettingsAction, CanSomeAction),
                CommandParameter = this
            });


            for (int i = 0; i < PortCount; i++)
            {
                var portItem = new MenuItemVm() {Header = string.Format(Resources.SID_Port_N, i)};
                portItem.Children.AddRange(GetFreePortSubMenuItems());

                menu.Add(portItem);
            }

            return menu;
        }

        private List<MenuItemVm> GetFreePortSubMenuItems()
        {
            var freePortSubMenuItems = new List<MenuItemVm>();

            freePortSubMenuItems.Add(new MenuItemVm()
            {
                Header = Resources.SID_Attach_from_list,
                Command = new ContextMenuAction(AttachFromListAction, CanSomeAction),
                CommandParameter = this,
            });

            return freePortSubMenuItems;
        }

        private void RtuInformationAction(object param) { }
        private void ShowRtuAction(object param) { }
        private void RtuSettingsAction(object param) { }
        private void AttachFromListAction(object param) { }
        private bool CanSomeAction(object param) { return true;}
    }
}