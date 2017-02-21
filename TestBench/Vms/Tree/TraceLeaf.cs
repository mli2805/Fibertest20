using System.Collections.Generic;
using System.Windows.Media;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.TestBench
{
    public class TraceLeaf : Leaf
    {
        public MonitoringState MonitoringState { get; set; }
        public FiberState TraceState { get; set; }

        public ImageSource MonitoringPictogram => MonitoringState.GetPictogram();
        public ImageSource TraceStatePictogram => TraceState.GetPictogram();

        public int PortNumber { get; set; }

        protected override List<MenuItemVm> GetMenuItems()
        {
            var menu = new List<MenuItemVm>();

            menu.Add(new MenuItemVm()
            {
                Header = Resources.SID_Information,
                Command = new ContextMenuAction(TraceInformationAction, CanSomeAction),
                CommandParameter = this
            });

            menu.Add(new MenuItemVm()
            {
                Header = Resources.SID_Show_trace,
                Command = new ContextMenuAction(TraceInformationAction, CanSomeAction),
                CommandParameter = this
            });

            menu.Add(new MenuItemVm()
            {
                Header = Resources.SID_Settings,
                Command = new ContextMenuAction(TraceInformationAction, CanSomeAction),
                CommandParameter = this
            });

            
            return menu;
        }

        private void TraceInformationAction(object param) { }
        private bool CanSomeAction(object param) { return true; }


    }
}