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
        private readonly IWindowManager _windowManager;
        private readonly ReadModel _readModel;
        public MonitoringState MonitoringState { get; set; }
        public FiberState TraceState { get; set; }

        public ImageSource MonitoringPictogram => MonitoringState.GetPictogram();
        public ImageSource TraceStatePictogram => TraceState.GetPictogram();

        public int PortNumber { get; set; }


        public TraceLeaf(IWindowManager windowManager, ReadModel readModel)
        {
            _windowManager = windowManager;
            _readModel = readModel;
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

            
            return menu;
        }

        private void TraceInformationAction(object param) { }
        private void ShowTraceAction(object param) { }

        private void AssignBaseRefsAction(object param)
        {
            var trace = _readModel.Traces.First(t => t.Id == Id);
            var vm = new BaseRefsAssignViewModel(trace, Parent.Title);
            _windowManager.ShowDialog(vm);

        }
        private bool CanSomeAction(object param) { return true; }


    }
}