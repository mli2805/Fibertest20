using System;
using System.Windows.Controls;
using Iit.Fibertest.Graph.Commands;

namespace Iit.Fibertest.TestBench
{
    public partial class MarkerControl
    {
        private bool CanUpdateRtu(object parameter) { return true; }
        private void AskUpdateRtu(object parameter)
        {
            var nodeId = (Guid)parameter;
            _owner.GraphVm.Command = new UpdateNode() { Id = nodeId };
        }

        private bool CanShowRtu(object parameter) { return true; }
        private void AskShowRtu(object parameter)
        {
            var nodeId = (Guid)parameter;
            _owner.GraphVm.Command = new UpdateNode() { Id = nodeId };
        }

        private bool CanRemoveRtu(object parameter) { return true; }
        private void AskRemoveRtu(object parameter)
        {
            var nodeId = (Guid)parameter;
            _owner.GraphVm.Command = new UpdateNode() { Id = nodeId };
        }

        private bool CanStartDefineTrace(object parameter) { return true; }
        private void StartDefineTrace(object parameter)
        {
            _mainMap.IsInTraceDefiningMode = true;
            _mainMap.StartNode = _marker;
        }
        private void OpenRtuContextMenu()
        {
            ContextMenu = new ContextMenu();
            ContextMenu.Items.Add(new MenuItem()
            {
                Header = "Информация",
                Command = new ContextMenuAction(AskUpdateRtu, CanUpdateRtu),
                CommandParameter = _marker.Id
            });
            ContextMenu.Items.Add(new MenuItem()
            {
                Header = "Показать RTU",
                Command = new ContextMenuAction(AskShowRtu, CanShowRtu),
                CommandParameter = _marker.Id
            });
            ContextMenu.Items.Add(new MenuItem()
            {
                Header = "Ориентиры",
                Command = new ContextMenuAction(AskLandmarks, CanLandmarks),
                CommandParameter = _marker.Id
            });
            ContextMenu.Items.Add(new MenuItem()
            {
                Header = "Удалить",
                Command = new ContextMenuAction(AskRemoveRtu, CanRemoveRtu),
                CommandParameter = _marker.Id
            });
            ContextMenu.Items.Add(new Separator());
            ContextMenu.Items.Add(new MenuItem()
            {
                Header = "Участок",
                Command = new ContextMenuAction(StartAddFiber, CanStartAddFiber),
                CommandParameter = _marker.Id
            });
            ContextMenu.Items.Add(new MenuItem()
            {
                Header = "Участок с узламии",
                Command = new ContextMenuAction(StartAddFiberWithNodes, CanStartAddFiberWithNodes),
                CommandParameter = _marker.Id
            });
            ContextMenu.Items.Add(new Separator());
            ContextMenu.Items.Add(new MenuItem()
            {
                Header = "Определить трассу",
                Command = new ContextMenuAction(StartDefineTrace, CanStartDefineTrace),
                CommandParameter = _marker.Id
            });
            ContextMenu.IsOpen = true;
        }
    }

}
