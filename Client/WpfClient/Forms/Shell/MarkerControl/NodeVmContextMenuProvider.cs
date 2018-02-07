using System.Windows.Controls;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class NodeVmContextMenuProvider
    {
        private readonly NodeVmActions _nodeVmActions;
        private readonly CommonVmActions _commonVmActions;
        private readonly NodeVmPermissions _nodeVmPermissions;

        public NodeVmContextMenuProvider(NodeVmActions nodeVmActions, CommonVmActions commonVmActions, NodeVmPermissions nodeVmPermissions)
        {
            _nodeVmActions = nodeVmActions;
            _commonVmActions = commonVmActions;
            _nodeVmPermissions = nodeVmPermissions;
        }

        public ContextMenu GetNodeContextMenu(MarkerControl marker)
        {
            var contextMenu = new ContextMenu();
            contextMenu.Items.Add(new MenuItem()
            {
                Header = Resources.SID_Information,
                Command = new ContextMenuAction(_nodeVmActions.AskUpdateNode, _nodeVmPermissions.CanUpdateNode),
                CommandParameter = marker
            });
            contextMenu.Items.Add(new MenuItem()
            {
                Header = Resources.SID_Add_Equipment,
                Command = new ContextMenuAction(_nodeVmActions.AskAddEquipment, _nodeVmPermissions.CanAddEquipment),
                CommandParameter = marker
            });
            contextMenu.Items.Add(new MenuItem()
            {
                Header = Resources.SID_Add_cable_reserve,
                Command = new ContextMenuAction(_nodeVmActions.AskAddCableReserve, _nodeVmPermissions.CanAddCableReserve),
                CommandParameter = marker
            });
            contextMenu.Items.Add(new MenuItem()
            {
                Header = Resources.SID_Landmarks,
                Command = new ContextMenuAction(_nodeVmActions.AskLandmarks, _nodeVmPermissions.CanLandmarks),
                CommandParameter = marker
            });
            contextMenu.Items.Add(new MenuItem()
            {
                Header = Resources.SID_Remove_node,
                Command = new ContextMenuAction(_nodeVmActions.AskRemoveNode, _nodeVmPermissions.CanRemoveNode),
                CommandParameter = marker
            });
            contextMenu.Items.Add(new Separator());
            contextMenu.Items.Add(new MenuItem()
            {
                Header = Resources.SID_Section,
                Command = new ContextMenuAction(_commonVmActions.StartAddFiber, _nodeVmPermissions.CanStartAddFiber),
                CommandParameter = marker
            });
            contextMenu.Items.Add(new MenuItem()
            {
                Header = Resources.SID_Section_with_nodes,
                Command = new ContextMenuAction(_commonVmActions.StartAddFiberWithNodes, _nodeVmPermissions.CanStartAddFiberWithNodes),
                CommandParameter = marker
            });
            return contextMenu;
        }

    }
}