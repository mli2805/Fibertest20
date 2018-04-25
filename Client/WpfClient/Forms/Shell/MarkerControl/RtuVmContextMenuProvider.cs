using System.Linq;
using System.Windows.Controls;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class RtuVmContextMenuProvider
    {
        private readonly RtuVmActions _rtuVmActions;
        private readonly CommonVmActions _commonVmActions;
        private readonly RtuVmPermissions _rtuVmPermissions;
        private readonly CurrentUser _currentUser;
        private readonly Model _readModel;

        public RtuVmContextMenuProvider(RtuVmActions rtuVmActions, CommonVmActions commonVmActions, RtuVmPermissions rtuVmPermissions, 
            CurrentUser currentUser, Model readModel)
        {
            _rtuVmActions = rtuVmActions;
            _commonVmActions = commonVmActions;
            _rtuVmPermissions = rtuVmPermissions;
            _currentUser = currentUser;
            _readModel = readModel;
        }

        public ContextMenu GetRtuContextMenu(MarkerControl marker)
        {
            var contextMenu = new ContextMenu();
            var rtuNodeId = marker.GMapMarker.Id;
            var rtu = _readModel.Rtus.First(r => r.NodeId == rtuNodeId);
            var user = _readModel.Users.First(u => u.UserId == _currentUser.UserId);

            contextMenu.Items.Add(new MenuItem()
            {
                Header = Resources.SID_Information,
                Command = new ContextMenuAction(_rtuVmActions.AskUpdateRtu, _rtuVmPermissions.CanUpdateRtu),
                CommandParameter = marker
            });
            contextMenu.Items.Add(new MenuItem()
            {
                Header = Resources.SID_Remove,
                Command = new ContextMenuAction(_rtuVmActions.AskRemoveRtu, _rtuVmPermissions.CanRemoveRtu),
                CommandParameter = marker
            });
            contextMenu.Items.Add(new Separator());
            contextMenu.Items.Add(new MenuItem()
            {
                Header = Resources.SID_Section,
                Command = new ContextMenuAction(_commonVmActions.StartAddFiber, _rtuVmPermissions.CanStartAddFiber),
                CommandParameter = marker
            });
            contextMenu.Items.Add(new MenuItem()
            {
                Header = Resources.SID_Section_with_nodes,
                Command = new ContextMenuAction(_commonVmActions.StartAddFiberWithNodes, _rtuVmPermissions.CanStartAddFiberWithNodes),
                CommandParameter = marker
            });
            contextMenu.Items.Add(new Separator());
            contextMenu.Items.Add(new MenuItem()
            {
                Header = Resources.SID_Define_trace,
                Command = new ContextMenuAction(_rtuVmActions.StartDefineTrace, _rtuVmPermissions.CanStartDefineTrace),
                CommandParameter = marker
            });
            contextMenu.Items.Add(new MenuItem()
            {
                Header = Resources.SID_Define_trace_step_by_step,
                Command = new ContextMenuAction(_rtuVmActions.StartDefineTraceStepByStep, _rtuVmPermissions.CanStartDefineTraceStepByStep),
                CommandParameter = marker
            });
            contextMenu.Items.Add(new MenuItem()
            {
                Header = Resources.SID_Hide_traces,
                Command = new ContextMenuAction(_rtuVmActions.HideTraces, _rtuVmPermissions.CanHideTraces),
                CommandParameter = marker,
                IsChecked = user.HiddenRtus.Contains(rtu.Id),
            });
            return contextMenu;
        }
    }
}