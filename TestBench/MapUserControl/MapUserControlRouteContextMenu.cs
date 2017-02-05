using System.Windows.Controls;
using GMap.NET.WindowsPresentation;
using Iit.Fibertest.Graph.Commands;

namespace Iit.Fibertest.TestBench
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
            GraphVm.Request = new UpdateFiber() { Id = route.Id };
        }

        private bool CanAddNodeIntoFiber(object parameter)
        {
            return true;
        }
        private void AskAddNodeIntoFiber(object parameter)
        {
            var route = (GMapRoute)parameter;
            GraphVm.Request = new AddNodeIntoFiber() { FiberId = route.Id };
        }

        private bool CanRemoveFiber(object parameter)
        {
            return true;
        }
        private void AskRemoveFiber(object parameter)
        {
            var route = (GMapRoute)parameter;
            GraphVm.Request = new RemoveFiber() { Id = route.Id };
        }

        private void Route_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "AskContextMenu")
            {
                var route = (GMapRoute)sender;
                route.ContextMenu = new ContextMenu();
                route.ContextMenu.Items.Add(new MenuItem()
                {
                    Header = "Информация",
                    Command = new ContextMenuAction(AskUpdateFiber, CanUpdateFiber),
                    CommandParameter = route
                });
                route.ContextMenu.Items.Add(new MenuItem()
                {
                    Header = "Добавить узел",
                    Command = new ContextMenuAction(AskAddNodeIntoFiber, CanAddNodeIntoFiber),
                    CommandParameter = route
                });
                route.ContextMenu.Items.Add(new MenuItem()
                {
                    Header = "Удалить участок",
                    Command = new ContextMenuAction(AskRemoveFiber, CanRemoveFiber),
                    CommandParameter = route
                });
            }
        }

    }
}
