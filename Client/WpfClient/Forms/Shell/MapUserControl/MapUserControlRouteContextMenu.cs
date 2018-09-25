using System.Linq;
using System.Windows.Controls;
using GMap.NET.WindowsPresentation;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Requests;

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
        private bool CanUpdateFiber(object parameter)
        {
            return parameter != null;
        }

        private async void AskUpdateFiber(object parameter)
        {
            var route = (GMapRoute)parameter;
            await GraphReadModel.GrmFiberRequests.UpdateFiber(new RequestUpdateFiber() { Id = route.Id });
        }

        private bool CanAddNodeIntoFiber(object parameter)
        {
            if (GraphReadModel.CurrentUser.Role > Role.Root || parameter == null)
                return false;
            var route = (GMapRoute)parameter;
            return !GraphReadModel.GrmNodeRequests.IsFiberContainedInAnyTraceWithBase(route.Id);
        }
        private async void AskAddNodeIntoFiber(object parameter)
        {
            var route = (GMapRoute)parameter;
            var position = MainMap.FromLocalToLatLng(MainMap.ContextMenuPoint);
            await GraphReadModel.GrmNodeRequests.AddNodeIntoFiber(new RequestAddNodeIntoFiber() { FiberId = route.Id, InjectionType = EquipmentType.EmptyNode, Position = position });
        }

        private bool CanAddAdjustmentNodeIntoFiber(object parameter)
        {
            return GraphReadModel.CurrentUser.Role <= Role.Root && parameter != null;
        }
        private async void AskAddAdjustmentNodeIntoFiber(object parameter)
        {
            var route = (GMapRoute)parameter;
            var position = MainMap.FromLocalToLatLng(MainMap.ContextMenuPoint);
            await GraphReadModel.GrmNodeRequests.AddNodeIntoFiber(new RequestAddNodeIntoFiber() { FiberId = route.Id, InjectionType = EquipmentType.AdjustmentPoint, Position = position });
        }

        private bool CanRemoveFiber(object parameter)
        {
            if (GraphReadModel.CurrentUser.Role > Role.Root || parameter == null)
                return false;
            var fiberVm = GraphReadModel.Data.Fibers.FirstOrDefault(f => f.Id == ((GMapRoute)parameter).Id);
            return fiberVm?.State == FiberState.NotInTrace;
        }

        private async void AskRemoveFiber(object parameter)
        {
            var route = (GMapRoute)parameter;
            await GraphReadModel.GrmFiberRequests.RemoveFiber(new RemoveFiber() { FiberId = route.Id });
        }

        private void Route_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == @"AskContextMenu")
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
