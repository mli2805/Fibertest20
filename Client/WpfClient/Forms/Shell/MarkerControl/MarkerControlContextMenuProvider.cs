using System;
using System.Windows.Controls;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class MarkerControlContextMenuProvider
    {
        private readonly MarkerControlActions _markerControlActions;
        private readonly MarkerControlPermissions _markerControlPermissions;

        public MarkerControlContextMenuProvider(MarkerControlActions markerControlActions, MarkerControlPermissions markerControlPermissions)
        {
            _markerControlActions = markerControlActions;
            _markerControlPermissions = markerControlPermissions;
        }

        public ContextMenu GetRtuContextMenu(Guid gMapMarkerId)
        {
            var contextMenu = new ContextMenu();
            contextMenu.Items.Add(new MenuItem()
            {
                Header = Resources.SID_Information,
                Command = new ContextMenuAction(_markerControlActions.AskUpdateRtu, _markerControlPermissions.CanUpdateRtu),
                CommandParameter = gMapMarkerId
            });
            contextMenu.Items.Add(new MenuItem()
            {
                Header = Resources.SID_Remove,
                Command = new ContextMenuAction(_markerControlActions.AskRemoveRtu, _markerControlPermissions.CanRemoveRtu),
                CommandParameter = gMapMarkerId
            });
            contextMenu.Items.Add(new Separator());
            contextMenu.Items.Add(new MenuItem()
            {
                Header = Resources.SID_Section,
                Command = new ContextMenuAction(_markerControlActions.StartAddFiber, _markerControlPermissions.CanStartAddFiber),
                CommandParameter = gMapMarkerId
            });
            contextMenu.Items.Add(new MenuItem()
            {
                Header = Resources.SID_Section_with_nodes,
                Command = new ContextMenuAction(_markerControlActions.StartAddFiberWithNodes, _markerControlPermissions.CanStartAddFiberWithNodes),
                CommandParameter = gMapMarkerId
            });
            contextMenu.Items.Add(new Separator());
            contextMenu.Items.Add(new MenuItem()
            {
                Header = Resources.SID_Define_trace,
                Command = new ContextMenuAction(_markerControlActions.StartDefineTrace, _markerControlPermissions.CanStartDefineTrace),
                CommandParameter = gMapMarkerId
            });
            contextMenu.Items.Add(new MenuItem()
            {
                Header = Resources.SID_Define_trace_step_by_step,
                Command = new ContextMenuAction(_markerControlActions.StartDefineTraceStepByStep, _markerControlPermissions.CanStartDefineTraceStepByStep),
                CommandParameter = gMapMarkerId
            });
            return contextMenu;
        }

        public ContextMenu GetNodeContextMenu(Guid markerId)
        {
            var contextMenu = new ContextMenu();
            contextMenu.Items.Add(new MenuItem()
            {
                Header = Resources.SID_Information,
                Command = new ContextMenuAction(_markerControlActions.AskUpdateNode, _markerControlPermissions.CanUpdateNode),
                CommandParameter = markerId
            });
            contextMenu.Items.Add(new MenuItem()
            {
                Header = Resources.SID_Add_Equipment,
                Command = new ContextMenuAction(_markerControlActions.AskAddEquipment, _markerControlPermissions.CanAddEquipment),
                CommandParameter = markerId
            });
            contextMenu.Items.Add(new MenuItem()
            {
                Header = Resources.SID_Add_cable_reserve,
                Command = new ContextMenuAction(_markerControlActions.AskAddCableReserve, _markerControlPermissions.CanAddCableReserve),
                CommandParameter = markerId
            });
            contextMenu.Items.Add(new MenuItem()
            {
                Header = Resources.SID_Landmarks,
                Command = new ContextMenuAction(_markerControlActions.AskLandmarks, _markerControlPermissions.CanLandmarks),
                CommandParameter = markerId
            });
            contextMenu.Items.Add(new MenuItem()
            {
                Header = Resources.SID_Remove_node,
                Command = new ContextMenuAction(_markerControlActions.AskRemoveNode, _markerControlPermissions.CanRemoveNode),
                CommandParameter = markerId
            });
            contextMenu.Items.Add(new Separator());
            contextMenu.Items.Add(new MenuItem()
            {
                Header = Resources.SID_Section,
                Command = new ContextMenuAction(_markerControlActions.StartAddFiber, _markerControlPermissions.CanStartAddFiber),
                CommandParameter = markerId
            });
            contextMenu.Items.Add(new MenuItem()
            {
                Header = Resources.SID_Section_with_nodes,
                Command = new ContextMenuAction(_markerControlActions.StartAddFiberWithNodes, _markerControlPermissions.CanStartAddFiberWithNodes),
                CommandParameter = markerId
            });
            return contextMenu;
        }

    }
}