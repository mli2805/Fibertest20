using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForClientInterface;
using Serilog;

namespace Iit.Fibertest.Client
{
    public class RtuLeaf : Leaf, IPortOwner
    {
        private readonly IniFile _iniFile35;
        private readonly ILogger _log;
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

        public ImageSource MonitoringPictogram => MonitoringState.GetPictogram();
        public ImageSource BopPictogram => BopState.GetPictogram();
        public ImageSource MainChannelPictogram => MainChannelState.GetPictogram();
        public ImageSource ReserveChannelPictogram => ReserveChannelState.GetPictogram();
        #endregion

        public int OwnPortCount { get; set; }
        public int FullPortCount { get; set; }
        public string Serial { get; set; }
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
                    extendedPortNumber < otau.FirstPortNumber + otau.PortCount)
                    return otau;
            }
            return null;
        }

        public RtuLeaf(ReadModel readModel, IWindowManager windowManager, IWcfServiceForClient c2DWcfManager,
            IniFile iniFile35, ILogger log, IMyLog logFile, PostOffice postOffice, FreePorts view)
            : base(readModel, windowManager, c2DWcfManager, postOffice)
        {
            _iniFile35 = iniFile35;
            _log = log;
            _logFile = logFile;
            ChildrenImpresario = new ChildrenImpresario(view);
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
            var vm = new RtuInitializeViewModel(Id, ReadModel, WindowManager, C2DWcfManager, _iniFile35, _log, _logFile);
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
        }

        private void ManualModeAction(object param)
        {
        }

        private void AutomaticModeAction(object param)
        {
        }

        private void RtuRemoveAction(object param)
        {
            C2DWcfManager.SendCommandAsObj(new RemoveRtu() { Id = Id });
        }

        private bool CanRtuRemoveAction(object param)
        {
            return !HasAttachedTraces;
        }

        private void DefineTraceStepByStepAction(object param)
        {
        }
    }
}