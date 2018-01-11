using System.Linq;
using System.Windows.Controls;
using GMap.NET.WindowsPresentation;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    /// <summary>
    /// on route creation subscribes on event that means 
    /// user clicked right mouse button on this route 
    /// 
    /// here we build ContextMenu for this particular route in runtime
    /// and here are reactions on MenuItems
    /// </summary>
    public partial class MapUserControl
    {
        private bool CanUpdateFiber(object parameter) { return true; }

        private void AskUpdateFiber(object parameter)
        {
            var route = (GMapRoute)parameter;
            GraphReadModel.Request = new RequestUpdateFiber() { Id = route.Id };
        }

        private bool CanAddNodeIntoFiber(object parameter)
        {
            return true;
        }
        private void AskAddNodeIntoFiber(object parameter)
        {
            var route = (GMapRoute)parameter;
            GraphReadModel.Request = new RequestAddNodeIntoFiber() { FiberId = route.Id, IsAdjustmentNode = false};
        }

        private bool CanAddAdjustmentNodeIntoFiber(object parameter)
        {
            return true;
        }
        private void AskAddAdjustmentNodeIntoFiber(object parameter)
        {
            var route = (GMapRoute)parameter;
            GraphReadModel.Request = new RequestAddNodeIntoFiber() { FiberId = route.Id, IsAdjustmentNode = true};
        }

        private bool CanRemoveFiber(object parameter)
        {
            if (parameter == null)
                return false;
            var fiberVm = GraphReadModel.Fibers.FirstOrDefault(f => f.Id == ((GMapRoute)parameter).Id);
            if (fiberVm == null) return false;
            return fiberVm.State == FiberState.NotInTrace;
        }

        private void AskRemoveFiber(object parameter)
        {
            var route = (GMapRoute)parameter;
            GraphReadModel.Request = new RemoveFiber() { Id = route.Id };
        }

        private void Route_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "AskContextMenu")
            {
                var route = (GMapRoute)sender;
                SetupContextMenu(route);
            }
        }

        private void SetupContextMenu(GMapRoute route)
        {
            route.ContextMenu = new ContextMenu();
            route.ContextMenu.Items.Add(new MenuItem()
            {
                Header = StringResources.Resources.SID_Information,
                Command = new ContextMenuAction(AskUpdateFiber, CanUpdateFiber),
                CommandParameter = route
            });
            route.ContextMenu.Items.Add(new MenuItem()
            {
                Header = StringResources.Resources.SID_Add_node,
                Command = new ContextMenuAction(AskAddNodeIntoFiber, CanAddNodeIntoFiber),
                CommandParameter = route
            });
            route.ContextMenu.Items.Add(new MenuItem()
            {
                Header = StringResources.Resources.SID_Add_adjustment_point,
                Command = new ContextMenuAction(AskAddAdjustmentNodeIntoFiber, CanAddAdjustmentNodeIntoFiber),
                CommandParameter = route
            });
            route.ContextMenu.Items.Add(new MenuItem()
            {
                Header = StringResources.Resources.SID_Remove_section,
                Command = new ContextMenuAction(AskRemoveFiber, CanRemoveFiber),
                CommandParameter = route
            });
        }
    }
}
