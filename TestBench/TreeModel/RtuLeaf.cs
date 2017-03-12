using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.TestBench
{
    public interface IPortOwner
    {
        ChildrenPorts ChildrenPorts { get; }
    }

    public class RtuLeaf : Leaf, IPortOwner
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

        public bool HasAttachedTraces => Children.Any(l => l is TraceLeaf && ((TraceLeaf) l).PortNumber > 0);

        public IPortOwner GetOwnerOfExtendedPort(int extendedPortNumber)
        {
            if (extendedPortNumber <= OwnPortCount)
                return this;
            foreach (var child in Children)
            {
                var otau = child as OtauLeaf;
                if (otau != null &&
                    extendedPortNumber >= otau.FirstPortNumber &&
                    extendedPortNumber < otau.FirstPortNumber + otau.PortCount)
                    return otau;
            }
            return null;
        }

        public RtuLeaf(ReadModel readModel, IWindowManager windowManager, Bus bus, ViewSettings view) 
            : base(readModel, windowManager, bus)
        {
            ChildrenPorts = new ChildrenPorts(Children, view);
        }

        public void RemoveFreePorts()
        {
            RemoveFreePorts(this);
        }

        private void RemoveFreePorts(Leaf owner)
        {
            foreach (var child in owner.Children.ToList())
            {
                if (child is PortLeaf)
                    owner.Children.Remove(child);
                if (child is OtauLeaf)
                    RemoveFreePorts(child);
            }
        }

        public void RestoreFreePorts()
        {
            var ports = new Leaf[OwnPortCount];
            var notAttachedTraces = new List<Leaf>();
            foreach (var child in Children)
            {
                if (child is OtauLeaf)
                    ports[((OtauLeaf) child).MasterPort - 1] = child;
                if (child is TraceLeaf)
                {
                    var portNumber = ((TraceLeaf) child).PortNumber;
                    if (portNumber > 0)
                        ports[portNumber - 1] = child;
                    else
                        notAttachedTraces.Add(child);
                }
            }
            for (int i = 0; i < ports.Length; i++)
            {
                if (ports[i] == null)
                    ports[i] = new PortLeaf(ReadModel, WindowManager, Bus, this, i + 1);
            }
            Children.Clear();
            ports.ToList().ForEach(t => Children.Add(t));
            notAttachedTraces.ForEach(t => Children.Add(t));
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
                Bus.SendCommand(vm.Command);
        }

        private void ShowRtuAction(object param)
        {
        }

        public void RtuSettingsAction(object param)
        {
            var vm = new RtuInitializeViewModel(Id, ReadModel, Bus);
            WindowManager.ShowDialog(vm);
        }

        private void RtuStateAction(object param)
        {
        }

        private void RtuLandmarksAction(object param)
        {
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
            Bus.SendCommand(new RemoveRtu() {Id = Id});
        }

        private bool CanRtuRemoveAction(object param)
        {
            return !HasAttachedTraces;
        }

        private void DefineTraceAction(object param)
        {
        }

        private void DefineTraceStepByStepAction(object param)
        {
        }

        public ChildrenPorts ChildrenPorts { get; }
    }

    public class ViewSettings : PropertyChangedBase
    {
        private PortDisplayMode _portDisplayMode;

        public PortDisplayMode PortDisplayMode
        {
            get { return _portDisplayMode; }
            set
            {
                if (value == _portDisplayMode) return;
                _portDisplayMode = value;
                NotifyOfPropertyChange();
            }
        }
    }

    public sealed class ChildrenPorts : PropertyChangedBase
    {
        private readonly ViewSettings _viewSettings;

        public ChildrenPorts(ObservableCollection<Leaf> children, ViewSettings viewSettings)
        {
            Children = children;

            _viewSettings = viewSettings;
            viewSettings.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ViewSettings.PortDisplayMode))
                    NotifyOfPropertyChange(nameof(EffectiveChildren));
            };
        }

        public ObservableCollection<Leaf> EffectiveChildren
            => _viewSettings.PortDisplayMode == PortDisplayMode.All ? Children : AttachedChildren;

        public ObservableCollection<Leaf> Children { get; } 
        public ObservableCollection<Leaf> AttachedChildren { get; } = new ObservableCollection<Leaf>();

    }
    public enum PortDisplayMode
    {
        All, Attached
    }
}