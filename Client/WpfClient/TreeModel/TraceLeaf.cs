using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForClientInterface;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public class TraceLeaf : Leaf
    {
        private int _portNumber;
        public int PortNumber
        {
            get { return _portNumber; }
            set
            {
                if (value == _portNumber) return;
                _portNumber = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(IconsVisibility));
                NotifyOfPropertyChange(nameof(LeftMargin));
                NotifyOfPropertyChange(nameof(Name));
            }
        }
        public int ExtendedPortNumber => Parent is OtauLeaf ? ((OtauLeaf)Parent).FirstPortNumber + PortNumber - 1 : PortNumber;

        public int LeftMargin => PortNumber < 1 ? 78 : Parent is RtuLeaf ? 53 : 74;
        public Visibility IconsVisibility => PortNumber > 0 ? Visibility.Visible : Visibility.Hidden;

        public override string Name
        {
            get { return PortNumber < 1 ? 
                            Title : 
                            Parent is OtauLeaf ?
                                string.Format(Resources.SID_Port_trace_on_otau, PortNumber, ExtendedPortNumber, Title) :
                                string.Format(Resources.SID_Port_trace, PortNumber, Title) ; }
            set { }
        }

        public MonitoringState MonitoringState { get; set; }
        public FiberState TraceState { get; set; }

        public ImageSource MonitoringPictogram => MonitoringState.GetPictogram();
        public ImageSource TraceStatePictogram => TraceState.GetPictogram();

        private readonly IniFile _iniFile;
        public TraceLeaf(IniFile iniFile, ReadModel readModel, 
            IWindowManager windowManager, IWcfServiceForClient c2DWcfManager, 
            PostOffice postOffice, IPortOwner parent) 
            : base(readModel, windowManager, c2DWcfManager, postOffice)
        {
            Parent = (Leaf)parent;
            _iniFile = iniFile;
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
                Command = new ContextMenuAction(AssignBaseRefsAction, CanAssignBaseRefsAction),
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
            {
                menu.Add(new MenuItemVm()
                {
                    Header = Resources.SID_Detach_trace,
                    Command = new ContextMenuAction(DetachTraceAction, CanSomeAction),
                    CommandParameter = this,
                });
            }
            else
            {
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
            }
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
                    Command = new ContextMenuAction(new PortExtensions().MeasurementClientAction, CanSomeAction),
                    CommandParameter = this
                });

                menu.Add(new MenuItemVm()
                {
                    Header = Resources.SID_Measurement__RFTS_Reflect_,
                    Command = new ContextMenuAction(MeasurementRftsReflectAction, CanSomeAction),
                    CommandParameter = this,
                });
            }
            return menu;
        }

        private void TraceInformationAction(object param)
        {
            var vm = new TraceInfoViewModel(ReadModel, C2DWcfManager, WindowManager, Id);
            WindowManager.ShowDialog(vm);
        }
        private void ShowTraceAction(object param) { }

        private bool CanAssignBaseRefsAction(object param)
        {
            var rtuLeaf = Parent is RtuLeaf ? (RtuLeaf)Parent : (RtuLeaf)Parent.Parent;
            return PortNumber < 1 || rtuLeaf.IsAvailable;
        }
        public void AssignBaseRefsAction(object param)
        {
            var trace = ReadModel.Traces.First(t => t.Id == Id);
            var vm = new BaseRefsAssignViewModel(_iniFile, ReadModel, C2DWcfManager, WindowManager);
            vm.Initialize(trace);
            WindowManager.ShowDialog(vm);
        }
        private void TraceStateAction(object param) { }
        private void TraceStatisticsAction(object param) { }

        private void TraceEventsAction(object param)
        {
            //TODO get last measurement sordata and pass it to vm
//            if (sorData.RftsEvents.MonitoringResult == (int)ComparisonReturns.NoFiber)
//            {
//                MessageBox.Show("No Fiber!", "Events");
//                return;
//            }
            var vm = new RftsEventsViewModel();
            WindowManager.ShowDialog(vm);
        }

        private void TraceLandmarksAction(object param)
        {
            var vm = new LandmarksViewModel(ReadModel, WindowManager);
            vm.Initialize(Id, false);
            WindowManager.ShowDialog(vm);
        }

        public void DetachTraceAction(object param)
        {
            C2DWcfManager.SendCommandAsObj(new DetachTrace() {TraceId = Id});
            
        }

        public void TraceCleanAction(object param)
        {
            C2DWcfManager.SendCommandAsObj(new CleanTrace() { Id = Id});
        }

        public void TraceRemoveAction(object param)
        {
            C2DWcfManager.SendCommandAsObj(new RemoveTrace() { Id = Id });
        }
        private void PreciseOutOfTurnMeasurementAction(object param) { }

        private void MeasurementRftsReflectAction(object param) { }
    }
}

