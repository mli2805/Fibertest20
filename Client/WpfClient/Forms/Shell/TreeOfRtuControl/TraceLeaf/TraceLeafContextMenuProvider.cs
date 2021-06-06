using System.Collections.Generic;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class TraceLeafContextMenuProvider
    {
        private readonly TraceLeafActions _traceLeafActions;
        private readonly TraceLeafActionsPermissions _traceLeafActionsPermissions;
        private readonly CommonActions _commonActions;

        public TraceLeafContextMenuProvider(TraceLeafActions traceLeafActions, 
            TraceLeafActionsPermissions traceLeafActionsPermissions, CommonActions commonActions)
        {
            _traceLeafActions = traceLeafActions;
            _traceLeafActionsPermissions = traceLeafActionsPermissions;
            _commonActions = commonActions;
        }

        public List<MenuItemVm> GetMenu(TraceLeaf traceLeaf)
        {
            var menu = new List<MenuItemVm>();
            menu.Add(new MenuItemVm()
            {
                Header = Resources.SID_Information,
                Command = new ContextMenuAction(_traceLeafActions.UpdateTrace, _traceLeafActionsPermissions.CanUpdateTrace),
                CommandParameter = traceLeaf
            });

            menu.Add(new MenuItemVm()
            {
                Header = Resources.SID_Show_trace,
                Command = new ContextMenuAction(_traceLeafActions.HighlightTrace, _traceLeafActionsPermissions.CanHighlightTrace),
                CommandParameter = traceLeaf
            });

            menu.Add(new MenuItemVm()
            {
                Header = Resources.SID_Base_refs_assignment,
                Command = new ContextMenuAction(_traceLeafActions.AssignBaseRefs, _traceLeafActionsPermissions.CanAssignBaseRefsAction),
                CommandParameter = traceLeaf
            });

            menu.Add(new MenuItemVm()
            {
                Header = Resources.SID_State,
                Command = new ContextMenuAction(_traceLeafActions.ShowTraceState, _traceLeafActionsPermissions.CanShowTraceState),
                CommandParameter = traceLeaf
            });

            menu.Add(new MenuItemVm()
            {
                Header = Resources.SID_Statistics,
                Command = new ContextMenuAction(_traceLeafActions.ShowTraceStatistics, _traceLeafActionsPermissions.CanShowTraceStatistics),
                CommandParameter = traceLeaf
            });

            menu.Add(new MenuItemVm()
            {
                Header = Resources.SID_Landmarks,
                Command = new ContextMenuAction(_traceLeafActions.ShowTraceLandmarks, _traceLeafActionsPermissions.CanShowTraceLandmarks),
                CommandParameter = traceLeaf
            });

            menu.Add(null);

            if (traceLeaf.PortNumber > 0)
            {
                menu.Add(new MenuItemVm()
                {
                    Header = Resources.SID_Unplug_trace,
                    Command = new ContextMenuAction(_traceLeafActions.DetachTrace, _traceLeafActionsPermissions.CanDetachTrace),
                    CommandParameter = traceLeaf,
                });
            }
            else
            {
                menu.Add(new MenuItemVm()
                {
                    Header = Resources.SID_Clean,
                    Command = new ContextMenuAction(_traceLeafActions.CleanTrace, _traceLeafActionsPermissions.CanCleanTrace),
                    CommandParameter = traceLeaf
                });

                menu.Add(new MenuItemVm()
                {
                    Header = Resources.SID_Remove,
                    Command = new ContextMenuAction(_traceLeafActions.RemoveTrace, _traceLeafActionsPermissions.CanRemoveTrace),
                    CommandParameter = traceLeaf
                });
            }

            if (traceLeaf.PortNumber > 0)
            {
                menu.Add(null);

                menu.Add(new MenuItemVm()
                {
                    Header = Resources.SID_Precise_monitoring_out_of_turn,
                    Command = new ContextMenuAction(_traceLeafActions.DoPreciseMeasurementOutOfTurn, _traceLeafActionsPermissions.CanDoPreciseMeasurementOutOfTurn),
                    CommandParameter = traceLeaf
                });

                menu.Add(new MenuItemVm()
                {
                    Header = Resources.SID_Measurement__Client_,
                    Command = new ContextMenuAction(_commonActions.MeasurementClientAction, _commonActions.CanMeasurementClientAction),
                    CommandParameter = traceLeaf
                });

                menu.Add(new MenuItemVm()
                {
                    Header = Resources.SID_Measurement__RFTS_Reflect_,
                    Command = new ContextMenuAction(_commonActions.MeasurementRftsReflectAction, _commonActions.CanMeasurementRftsReflectAction),
                    CommandParameter = traceLeaf,
                });
            }
            return menu;
        }

    }
}