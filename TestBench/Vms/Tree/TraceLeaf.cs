using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.TestBench
{
    public class TraceLeaf : Leaf
    {
        public int PortNumber { get; set; }
        public MonitoringState MonitoringState { get; set; }
        public FiberState TraceState { get; set; }

        public ImageSource MonitoringPictogram => MonitoringState.GetPictogram();
        public ImageSource TraceStatePictogram => TraceState.GetPictogram();


        public TraceLeaf(ReadModel readModel, IWindowManager windowManager, Bus bus) : base(readModel, windowManager, bus)
        {
        }

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
                Command = new ContextMenuAction(ShowTraceAction, CanSomeAction),
                CommandParameter = this
            });

            menu.Add(new MenuItemVm()
            {
                Header = Resources.SID_Base_refs_assignment,
                Command = new ContextMenuAction(AssignBaseRefsAction, CanSomeAction),
                CommandParameter = this
            });

            menu.Add(new MenuItemVm()
            {
                Header = Resources.SID_State,
                Command = new ContextMenuAction(TraceStateAction, CanSomeAction),
                CommandParameter = this
            });

            menu.Add(new MenuItemVm()
            {
                Header = Resources.SID_Statistics,
                Command = new ContextMenuAction(TraceStatisticsAction, CanSomeAction),
                CommandParameter = this
            });

            menu.Add(new MenuItemVm()
            {
                Header = Resources.SID_Events,
                Command = new ContextMenuAction(TraceEventsAction, CanSomeAction),
                CommandParameter = this
            });

            menu.Add(new MenuItemVm()
            {
                Header = Resources.SID_Landmarks,
                Command = new ContextMenuAction(TraceLandmarksAction, CanSomeAction),
                CommandParameter = this
            });

            menu.Add(null);

            if (PortNumber > 0)
                menu.Add(new MenuItemVm()
                {
                    Header = Resources.SID_Detach_trace,
                    Command = new ContextMenuAction(DetachTraceAction, CanSomeAction),
                    CommandParameter = this,
                });

            menu.Add(new MenuItemVm()
            {
                Header = Resources.SID_Clean,
                Command = new ContextMenuAction(TraceCleanAction, CanSomeAction),
                CommandParameter = this
            });

            menu.Add(new MenuItemVm()
            {
                Header = Resources.SID_Remove,
                Command = new ContextMenuAction(TraceRemoveAction, CanSomeAction),
                CommandParameter = this
            });

            if (PortNumber > 0)
            {
                menu.Add(null);

                menu.Add(new MenuItemVm()
                {
                    Header = Resources.SID_Presice_out_of_turn_measurement,
                    Command = new ContextMenuAction(PreciseOutOfTurnMeasurementAction, CanSomeAction),
                    CommandParameter = this
                });

                menu.Add(new MenuItemVm()
                {
                    Header = Resources.SID_Measurement__Client_,
                    Command = new ContextMenuAction(PortExtensions.MeasurementClientAction, CanSomeAction),
                    CommandParameter = this
                });

                menu.Add(new MenuItemVm()
                {
                    Header = Resources.SID_Measurement__RFTS_Reflect_,
                    Command = new ContextMenuAction(PortExtensions.MeasurementRftsReflectAction, CanSomeAction),
                    CommandParameter = this,
                });
            }
            return menu;
        }

        private void TraceInformationAction(object param)
        {
            var vm = new TraceInfoViewModel(ReadModel, Bus, WindowManager, Id);
            WindowManager.ShowDialog(vm);
        }
        private void ShowTraceAction(object param) { }

        private void AssignBaseRefsAction(object param)
        {
            var trace = ReadModel.Traces.First(t => t.Id == Id);
            var vm = new BaseRefsAssignViewModel(trace, ReadModel, Bus);
            WindowManager.ShowDialog(vm);
        }
        private void TraceStateAction(object param) { }
        private void TraceStatisticsAction(object param) { }
        private void TraceEventsAction(object param) { }
        private void TraceLandmarksAction(object param) { }

        private void DetachTraceAction(object param)
        {
            Bus.SendCommand(new DetachTrace() {TraceId = Id});
        }
        private void TraceCleanAction(object param) { }
        private void TraceRemoveAction(object param) { }
        private void PreciseOutOfTurnMeasurementAction(object param) { }
    }
}

/*
        menu.Add(null);

        for (int i = 1; i <= PortCount; i++)
        {
            var portItem = new MenuItemVm() { Header = string.Format(Resources.SID_Port_N, i) };
            portItem.Children.AddRange(GetFreePortSubMenuItems());

            menu.Add(portItem);
        }
*/
