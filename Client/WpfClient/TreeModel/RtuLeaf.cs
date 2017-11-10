using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Client.MonitoringSettings;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class RtuLeaf : Leaf, IPortOwner
    {
        private readonly ILifetimeScope _globalScope;
        private readonly IMyLog _logFile;

        #region Pictograms
        private MonitoringState _monitoringState;
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

        private RtuPartState _bopState;
        public RtuPartState BopState
        {
            get { return _bopState; }
            set
            {
                if (value == _bopState) return;
                _bopState = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(BopPictogram));
            }
        }

        private RtuPartState _mainChannelState;
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

        private RtuPartState _reserveChannelState;
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

        public bool IsAvailable => MainChannelState == RtuPartState.Normal ||
                                   ReserveChannelState == RtuPartState.Normal;

        public ImageSource MonitoringPictogram => MonitoringState.GetPictogram();
        public ImageSource BopPictogram => BopState.GetPictogram();
        public ImageSource MainChannelPictogram => MainChannelState.GetPictogram();
        public ImageSource ReserveChannelPictogram => ReserveChannelState.GetPictogram();
        #endregion

        public int OwnPortCount { get; set; }
        public int FullPortCount { get; set; }
        public string Serial { get; set; }
        public NetAddress OtauNetAddress { get; set; }
        public override string Name => Title;

        public ChildrenImpresario ChildrenImpresario { get; }
        public bool HasAttachedTraces => ChildrenImpresario.Children.Any(l => l is TraceLeaf && ((TraceLeaf)l).PortNumber > 0);
        public int TraceCount => ChildrenImpresario.Children.Count(c => c is TraceLeaf) +
                ChildrenImpresario.Children.Where(c => c is OtauLeaf).Sum(otauLeaf => ((OtauLeaf)otauLeaf).TraceCount);

        public IPortOwner GetOwnerOfExtendedPort(int extendedPortNumber)
        {
            if (extendedPortNumber <= OwnPortCount)
                return this;
            foreach (var child in ChildrenImpresario.Children)
            {
                var otau = child as OtauLeaf;
                if (otau != null &&
                    extendedPortNumber >= otau.FirstPortNumber &&
                    extendedPortNumber < otau.FirstPortNumber + otau.OwnPortCount)
                    return otau;
            }
            return null;
        }

        public RtuLeaf(ILifetimeScope globalScope, IMyLog logFile, ReadModel readModel, IWindowManager windowManager, IWcfServiceForClient c2DWcfManager,
            PostOffice postOffice, FreePorts view)
            : base(readModel, windowManager, c2DWcfManager, postOffice)
        {
            _globalScope = globalScope;
            _logFile = logFile;
            ChildrenImpresario = new ChildrenImpresario(view);

            Title = Resources.SID_noname_RTU;
            Color = Brushes.DarkGray;
            IsExpanded = true;
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
                Header = Resources.SID_Network_settings,
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
                Command = new ContextMenuAction(ManualModeAction, CanStopMonitoring),
                CommandParameter = this
            });

            menu.Add(new MenuItemVm()
            {
                Header = Resources.SID_Automatic_mode,
                Command = new ContextMenuAction(AutomaticModeAction, CanStartMonitoring),
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
                Header = Resources.SID_Define_trace_step_by_step,
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
                C2DWcfManager.SendCommandAsObj(vm.Command);
        }

        private void ShowRtuAction(object param)
        {
            PostOffice.Message = new CenterToRtu() { RtuId = Id };
        }

        public void RtuSettingsAction(object param)
        {
            var localScope = _globalScope.BeginLifetimeScope(ctx => ctx.RegisterInstance(this));
            var vm = localScope.Resolve<RtuInitializeViewModel>();
            WindowManager.ShowDialog(vm);
        }

        private void RtuStateAction(object param)
        {
        }

        private void RtuLandmarksAction(object param)
        {
            var vm = new LandmarksViewModel(ReadModel, WindowManager);
            vm.Initialize(Id, true);
            WindowManager.ShowDialog(vm);
        }

        private void MonitoringSettingsAction(object param)
        {

            var model = new MonitoringSettingsManager(this).PrepareMonitoringSettingsModel();
            var vm = new MonitoringSettingsViewModel(model);
            WindowManager.ShowDialog(vm);
        }

        private async void ManualModeAction(object param)
        {
            var result = await C2DWcfManager.StopMonitoringAsync(new StopMonitoringDto() {RtuId = Id});
            _logFile.AppendLine($@"Stop monitoring result - {result}");
            if (result)
                MonitoringState = MonitoringState.Off;
            var vm = new NotificationViewModel(
                result ? Resources.SID_Information : Resources.SID_Error_, 
                result ? Resources.SID_RTU_is_turned_into_manual_mode : Resources.SID_Cannot_turn_RTU_into_manual_mode);
            WindowManager.ShowDialog(vm);
        }

        private async void AutomaticModeAction(object param)
        {
            var result = await C2DWcfManager.StartMonitoringAsync(new StartMonitoringDto() {RtuId = Id});
            _logFile.AppendLine($@"Start monitoring result - {result}");
            if (result)
                MonitoringState = MonitoringState.On;
            var vm = new NotificationViewModel(
                result ? Resources.SID_Information : Resources.SID_Error_, 
                result ? Resources.SID_RTU_is_turned_into_automatic_mode : Resources.SID_Cannot_turn_RTU_into_automatic_mode);
            WindowManager.ShowDialog(vm);
        }

        private void RtuRemoveAction(object param)
        {
            C2DWcfManager.SendCommandAsObj(new RemoveRtu() { Id = Id });
        }

        private bool CanRtuRemoveAction(object param)
        {
            return !HasAttachedTraces;
        }

        private bool CanStartMonitoring(object param)
        {
            return MonitoringState == MonitoringState.Off;
        }

        private bool CanStopMonitoring(object param)
        {
            return MonitoringState == MonitoringState.On;
        }

        private void DefineTraceStepByStepAction(object param)
        {
        }

    }
}