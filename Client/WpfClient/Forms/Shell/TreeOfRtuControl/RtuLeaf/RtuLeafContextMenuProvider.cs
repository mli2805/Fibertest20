using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class RtuLeafContextMenuProvider
    {
        private readonly CurrentUser _currentUser;
        private readonly Model _readModel;
        private readonly RtuLeafActions _rtuLeafActions;
        private readonly RtuLeafActionsPermissions _rtuLeafActionsPermissions;

        public RtuLeafContextMenuProvider(CurrentUser currentUser, Model readModel, RtuLeafActions rtuLeafActions, 
            RtuLeafActionsPermissions rtuLeafActionsPermissions)
        {
            _currentUser = currentUser;
            _readModel = readModel;
            _rtuLeafActions = rtuLeafActions;
            _rtuLeafActionsPermissions = rtuLeafActionsPermissions;
        }

        public List<MenuItemVm> GetMenu(RtuLeaf rtuLeaf)
        {
            var user = _readModel.Users.First(u => u.UserId == _currentUser.UserId);

            var menu = new List<MenuItemVm>();
            menu.Add(new MenuItemVm()
            {
                Header = Resources.SID_Information,
                Command = new ContextMenuAction(_rtuLeafActions.UpdateRtu, _rtuLeafActionsPermissions.CanUpdateRtu),
                CommandParameter = rtuLeaf
            });

            menu.Add(new MenuItemVm()
            {
                Header = Resources.SID_Show_RTU,
                Command = new ContextMenuAction(_rtuLeafActions.ShowRtu, _rtuLeafActionsPermissions.CanShowRtu),
                CommandParameter = rtuLeaf
            });

            menu.Add(new MenuItemVm()
            {
                Header = Resources.SID_Network_settings,
                Command = new ContextMenuAction(_rtuLeafActions.InitializeRtu, _rtuLeafActionsPermissions.CanInitializeRtu),
                CommandParameter = rtuLeaf
            });

            menu.Add(new MenuItemVm()
            {
                Header = Resources.SID_State,
                Command = new ContextMenuAction(_rtuLeafActions.ShowRtuState, _rtuLeafActionsPermissions.CanShowRtuState),
                CommandParameter = rtuLeaf
            });

            menu.Add(new MenuItemVm()
            {
                Header = Resources.SID_Landmarks,
                Command = new ContextMenuAction(_rtuLeafActions.ShowRtuLandmarks, _rtuLeafActionsPermissions.CanShowRtuLandmarks),
                CommandParameter = rtuLeaf
            });

            menu.Add(null);

            menu.Add(new MenuItemVm()
            {
                Header = Resources.SID_Monitoring_settings,
                Command = new ContextMenuAction(_rtuLeafActions.ShowMonitoringSettings, _rtuLeafActionsPermissions.CanShowMonitoringSettings),
                CommandParameter = rtuLeaf
            });

            menu.Add(new MenuItemVm()
            {
                Header = Resources.SID_Manual_mode,
                Command = new ContextMenuAction(_rtuLeafActions.StopMonitoring, _rtuLeafActionsPermissions.CanStopMonitoring),
                CommandParameter = rtuLeaf
            });

            menu.Add(new MenuItemVm()
            {
                Header = Resources.SID_Automatic_mode,
                Command = new ContextMenuAction(_rtuLeafActions.StartMonitoring, _rtuLeafActionsPermissions.CanStartMonitoring),
                CommandParameter = rtuLeaf
            });

            menu.Add(null);

            menu.Add(new MenuItemVm()
            {
                Header = Resources.SID_Remove,
                Command = new ContextMenuAction(_rtuLeafActions.RemoveRtu, _rtuLeafActionsPermissions.CanRemoveRtu),
                CommandParameter = rtuLeaf
            });

            menu.Add(null);

            menu.Add(new MenuItemVm()
            {
                Header = Resources.SID_Define_trace_step_by_step,
                Command = new ContextMenuAction(_rtuLeafActions.DefineTraceStepByStep, _rtuLeafActionsPermissions.CanDefineTraceStepByStep),
                CommandParameter = rtuLeaf
            });
            menu.Add(new MenuItemVm()
            {
                Header = Resources.SID_Hide_traces,
                Command = new ContextMenuAction(_rtuLeafActions.HideTraces, _rtuLeafActionsPermissions.CanHideTraces),
                CommandParameter = rtuLeaf,
                IsChecked = user.HiddenRtus.Contains(rtuLeaf.Id),
            }); return menu;
        }
    }
}