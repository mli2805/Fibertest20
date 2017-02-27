using System;
using System.Windows.Controls;

namespace Iit.Fibertest.TestBench
{
    public partial class MarkerControl
    {
        private bool CanUpdateRtu(object parameter) { return true; }
        private void AskUpdateRtu(object parameter)
        {
            var nodeId = (Guid)parameter;
            _owner.GraphReadModel.Request = new RequestUpdateRtu() { NodeId = nodeId };
        }

        private bool CanRemoveRtu(object parameter) { return true; }
        private void AskRemoveRtu(object parameter)
        {
            var nodeId = (Guid)parameter;
            _owner.GraphReadModel.Request = new RequestRemoveRtu() { NodeId = nodeId };
        }

        private bool CanStartDefineTrace(object parameter) { return true; }
        private void StartDefineTrace(object parameter)
        {
            _mainMap.IsInTraceDefiningMode = true;
            _mainMap.StartNode = _marker;
        }
        private bool CanStartDefineTraceStepByStep(object parameter) { return false; }
        private void StartDefineTraceStepByStep(object parameter)
        {
            _mainMap.IsInTraceDefiningMode = true;
            _mainMap.StartNode = _marker;
        }
        private void OpenRtuContextMenu()
        {
            ContextMenu = new ContextMenu();
            ContextMenu.Items.Add(new MenuItem()
            {
                Header = StringResources.Resources.SID_Information,
                Command = new ContextMenuAction(AskUpdateRtu, CanUpdateRtu),
                CommandParameter = _marker.Id
            });
            ContextMenu.Items.Add(new MenuItem()
            {
                Header = StringResources.Resources.SID_Remove,
                Command = new ContextMenuAction(AskRemoveRtu, CanRemoveRtu),
                CommandParameter = _marker.Id
            });
            ContextMenu.Items.Add(new Separator());
            ContextMenu.Items.Add(new MenuItem()
            {
                Header = StringResources.Resources.SID_Section,
                Command = new ContextMenuAction(StartAddFiber, CanStartAddFiber),
                CommandParameter = _marker.Id
            });
            ContextMenu.Items.Add(new MenuItem()
            {
                Header = StringResources.Resources.SID_Section_with_nodes,
                Command = new ContextMenuAction(StartAddFiberWithNodes, CanStartAddFiberWithNodes),
                CommandParameter = _marker.Id
            });
            ContextMenu.Items.Add(new Separator());
            ContextMenu.Items.Add(new MenuItem()
            {
                Header = StringResources.Resources.SID_Define_trace,
                Command = new ContextMenuAction(StartDefineTrace, CanStartDefineTrace),
                CommandParameter = _marker.Id
            });
            ContextMenu.Items.Add(new MenuItem()
            {
                Header = StringResources.Resources.SID_Define_trace_strp_by_step,
                Command = new ContextMenuAction(StartDefineTraceStepByStep, CanStartDefineTraceStepByStep),
                CommandParameter = _marker.Id
            });
            ContextMenu.IsOpen = true;
        }
    }
}
