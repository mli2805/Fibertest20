using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.TestBench
{
    public class RtuLeaf : Leaf
    {
        private RtuPartState _mainChannelState;
        private MonitoringState _monitoringState;
        private RtuPartState _reserveChannelState;

        public MonitoringState MonitoringState
        {
            get { return _monitoringState; }
            set
            {
                if (value == _monitoringState) return;
                _monitoringState = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(MonitoringPictogram));
            }
        }

        public RtuPartState BopState { get; set; }

        public RtuPartState MainChannelState
        {
            get { return _mainChannelState; }
            set
            {
                if (value == _mainChannelState) return;
                _mainChannelState = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(MainChannelPictogram));
            }
        }

        public RtuPartState ReserveChannelState
        {
            get { return _reserveChannelState; }
            set
            {
                if (value == _reserveChannelState) return;
                _reserveChannelState = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(ReserveChannelPictogram));
            }
        }

        public ImageSource MonitoringPictogram => MonitoringState.GetPictogram();
        public ImageSource BopPictogram => BopState.GetPictogram();
        public ImageSource MainChannelPictogram => MainChannelState.GetPictogram();
        public ImageSource ReserveChannelPictogram => ReserveChannelState.GetPictogram();

        public int OwnPortCount { get; set; }
        public int FullPortCount { get; set; }
        public override string Name => Title;

        public RtuLeaf(ReadModel readModel, IWindowManager windowManager, Bus bus) : base(readModel, windowManager, bus)
        {
        }

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

            menu.Add(new MenuItemVm()
            {
                Header = Resources.SID_State,
                Command = new ContextMenuAction(RtuStateAction, CanSomeAction),
                CommandParameter = this
            });

            menu.Add(new MenuItemVm()
            {
                Header = Resources.SID_Landmarks,
                Command = new ContextMenuAction(RtuLandmarksAction, CanSomeAction),
                CommandParameter = this
            });

            menu.Add(null);

            menu.Add(new MenuItemVm()
            {
                Header = Resources.SID_Monitoring_settings,
                Command = new ContextMenuAction(MonitoringSettingsAction, CanSomeAction),
                CommandParameter = this
            });

            menu.Add(new MenuItemVm()
            {
                Header = Resources.SID_Manual_mode,
                Command = new ContextMenuAction(ManualModeAction, CanSomeAction),
                CommandParameter = this
            });

            menu.Add(new MenuItemVm()
            {
                Header = Resources.SID_Automatic_mode,
                Command = new ContextMenuAction(AutomaticModeAction, CanSomeAction),
                CommandParameter = this
            });

            menu.Add(null);

            menu.Add(new MenuItemVm()
            {
                Header = Resources.SID_Remove,
                Command = new ContextMenuAction(RtuRemoveAction, CanRtuRemoveAction),
                CommandParameter = this
            });

            menu.Add(null);

            menu.Add(new MenuItemVm()
            {
                Header = Resources.SID_Define_trace,
                Command = new ContextMenuAction(DefineTraceAction, CanSomeAction),
                CommandParameter = this
            });

            menu.Add(new MenuItemVm()
            {
                Header = Resources.SID_Define_trace_strp_by_step,
                Command = new ContextMenuAction(DefineTraceStepByStepAction, CanSomeAction),
                CommandParameter = this
            });

            return menu;
        }

        private void RtuInformationAction(object param)
        {
            var vm = new RtuUpdateViewModel(Id, ReadModel);
            WindowManager.ShowDialog(vm);
            if (vm.Command != null)
                Bus.SendCommand(vm.Command);
        }
        private void ShowRtuAction(object param) { }

        private void RtuSettingsAction(object param)
        {
            var vm = new RtuInitializeViewModel(Id, ReadModel, Bus);
            WindowManager.ShowDialog(vm);
        }
        private void RtuStateAction(object param) { }
        private void RtuLandmarksAction(object param) { }
        private void MonitoringSettingsAction(object param) { }
        private void ManualModeAction(object param) { }
        private void AutomaticModeAction(object param) { }

        private void RtuRemoveAction(object param)
        {
            Bus.SendCommand(new RemoveRtu() {Id = Id});
        }

        private bool CanRtuRemoveAction(object param)
        {
            return !Children.Any(l => l is TraceLeaf && ((TraceLeaf) l).PortNumber > 0);
        }
        private void DefineTraceAction(object param) { }
        private void DefineTraceStepByStepAction(object param) { }
    }
}