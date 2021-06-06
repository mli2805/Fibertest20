using System.Collections.Generic;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class RtuLeafContextMenuProvider
    {
        private readonly RtuLeafActions _rtuLeafActions;
        private readonly RtuLeafActionsPermissions _rtuLeafActionsPermissions;

        public RtuLeafContextMenuProvider(RtuLeafActions rtuLeafActions, RtuLeafActionsPermissions rtuLeafActionsPermissions)
        {
            _rtuLeafActions = rtuLeafActions;
            _rtuLeafActionsPermissions = rtuLeafActionsPermissions;
        }

        public List<MenuItemVm> GetMenu(RtuLeaf rtuLeaf)
        {
            var menu = new List<MenuItemVm>();
            menu.Add(new MenuItemVm()
            {
                Header = Resources.SID_Information,
                Command = new ContextMenuAction(_rtuLeafActions.ShowRtuInfoView, _rtuLeafActionsPermissions.CanShowRtuInfoView),
                CommandParameter = rtuLeaf
            });

            menu.Add(new MenuItemVm()
            {
                Header = Resources.SID_Show_RTU,
                Command = new ContextMenuAction(_rtuLeafActions.HigtlightRtu, _rtuLeafActionsPermissions.CanHighlightRtu),
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
                Header = Resources.SID_Detach_all_traces,
                Command = new ContextMenuAction(_rtuLeafActions.DetachAllTraces, _rtuLeafActionsPermissions.CanDetachAllTraces),
                CommandParameter = rtuLeaf
            });

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
            return menu;
        }
    }
}