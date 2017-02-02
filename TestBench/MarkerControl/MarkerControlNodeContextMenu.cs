using System;
using System.Windows.Controls;
using System.Windows.Input;
using Iit.Fibertest.Graph.Commands;

namespace Iit.Fibertest.TestBench
{
    public partial class MarkerControl
    {
        private bool CanUpdateNode(object parameter) { return true; }
        private void AskUpdateNode(object parameter)
        {
            var nodeId = (Guid)parameter;
            _owner.GraphVm.Command = new UpdateNode() { Id = nodeId };
        }

        private bool CanAddEquipment(object parameter) { return false; }
        private void AskAddEquipment(object parameter)
        {
        }

        private bool CanLandmarks(object parameter) { return false; }
        private void AskLandmarks(object parameter)
        {
        }

        private bool CanRemoveNode(object parameter) { return true; }
        private void AskRemoveNode(object parameter)
        {
            var nodeId = (Guid)parameter;
            _owner.GraphVm.Command = new RemoveNode() { Id = nodeId };
        }

        private bool CanStartAddFiber(object parameter) { return true; }
        private void StartAddFiber(object parameter)
        {
            _mainMap.IsFiberWithNodes = false;
            _mainMap.IsInFiberCreationMode = true;
            _mainMap.StartNode = _marker;
        }

        private bool CanStartAddFiberWithNodes(object parameter) { return true; }
        private void StartAddFiberWithNodes(object parameter)
        {
            _mainMap.IsFiberWithNodes = true;
            _mainMap.IsInFiberCreationMode = true;
            _mainMap.StartNode = _marker;
        }

        private void OpenNodeContextMenu()
        {
            ContextMenu = new ContextMenu();
            ContextMenu.Items.Add(new MenuItem()
            {
                Header = "Информация",
                Command = new ContextMenuAction(AskUpdateNode, CanUpdateNode),
                CommandParameter = _marker.Id
            });
            ContextMenu.Items.Add(new MenuItem()
            {
                Header = "Добавить оборудование",
                Command = new ContextMenuAction(AskAddEquipment, CanAddEquipment),
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
                Header = "Удалить узел",
                Command = new ContextMenuAction(AskRemoveNode, CanRemoveNode),
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
            ContextMenu.IsOpen = true;
        }
    }

}
