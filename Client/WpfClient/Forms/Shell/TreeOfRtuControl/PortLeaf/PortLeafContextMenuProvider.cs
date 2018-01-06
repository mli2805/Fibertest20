using System.Collections.Generic;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class PortLeafContextMenuProvider
    {
        private readonly PortLeafActions _portLeafActions;
        private readonly CommonActions _commonActions;

        public PortLeafContextMenuProvider(PortLeafActions portLeafActions, CommonActions commonActions)
        {
            _portLeafActions = portLeafActions;
            _commonActions = commonActions;
        }

        public List<MenuItemVm> GetMenu(PortLeaf portLeaf)
        {
            var menu = new List<MenuItemVm>();

            menu.AddRange(GetFreePortMenuItems(portLeaf));
            menu.AddRange(GetAnyPortMenuItems(portLeaf));

            return menu;
        }
        private IEnumerable<MenuItemVm> GetFreePortMenuItems(PortLeaf portLeaf)
        {
            yield return new MenuItemVm()
            {
                Header = Resources.SID_Attach_from_list,
                Command = new ContextMenuAction(_portLeafActions.AttachFromListAction, _portLeafActions.CanAttachTraceAction),
                CommandParameter = portLeaf,
            };

            yield return new MenuItemVm()
            {
                Header = Resources.SID_Attach_optical_switch,
                Command = new ContextMenuAction(_portLeafActions.AttachOtauAction, _portLeafActions.CanAttachOtauAction),
                CommandParameter = portLeaf,
            };
        }

        private IEnumerable<MenuItemVm> GetAnyPortMenuItems(PortLeaf portLeaf)
        {
            yield return null;

            yield return new MenuItemVm()
            {
                Header = Resources.SID_Measurement__Client_,
                Command = new ContextMenuAction(_commonActions.MeasurementClientAction, _commonActions.CanMeasurementClientAction),
                CommandParameter = portLeaf,
            };

            yield return new MenuItemVm()
            {
                Header = Resources.SID_Measurement__RFTS_Reflect_,
                Command = new ContextMenuAction(_commonActions.MeasurementRftsReflectAction, _commonActions.CanMeasurementRftsReflectAction),
                CommandParameter = portLeaf,
            };
        }


    }
}