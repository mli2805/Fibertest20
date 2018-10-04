using System.Windows.Controls;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class MapContextMenuProvider
    {
        private readonly MapActions _mapActions;

        public MapContextMenuProvider(MapActions mapActions)
        {
            _mapActions = mapActions;
        }

        public ContextMenu GetMapContextMenu()
        {
            var contextMenu = new ContextMenu();
            contextMenu.Items.Add(new MenuItem()
            {
                Header = Resources.SID_Add_node,
                Command = new ContextMenuAction(_mapActions.AddNodeOnClick, _mapActions.CanAddNode),
                CommandParameter = EquipmentType.EmptyNode,
            });

            contextMenu.Items.Add(new Separator());

            contextMenu.Items.Add(new MenuItem()
            {
                Header = Resources.SID_Add_node_with_cable_reserve,
                Command = new ContextMenuAction(_mapActions.AddNodeOnClick, _mapActions.CanAddNode),
                CommandParameter = EquipmentType.CableReserve,
            });

            contextMenu.Items.Add(new MenuItem()
            {
                Header = Resources.SID_Add_node_with_sleeve,
                Command = new ContextMenuAction(_mapActions.AddNodeOnClick, _mapActions.CanAddNode),
                CommandParameter = EquipmentType.Closure,
            });

            contextMenu.Items.Add(new MenuItem()
            {
                Header = Resources.SID_Add_node_with_cross,
                Command = new ContextMenuAction(_mapActions.AddNodeOnClick, _mapActions.CanAddNode),
                CommandParameter = EquipmentType.Cross,
            });

            contextMenu.Items.Add(new MenuItem()
            {
                Header = Resources.SID_Add_node_with_terminal,
                Command = new ContextMenuAction(_mapActions.AddNodeOnClick, _mapActions.CanAddNode),
                CommandParameter = EquipmentType.Terminal,
            });

            contextMenu.Items.Add(new MenuItem()
            {
                Header = Resources.SID_Add_node_with_other_equipment,
                Command = new ContextMenuAction(_mapActions.AddNodeOnClick, _mapActions.CanAddNode),
                CommandParameter = EquipmentType.Other,
            });

            contextMenu.Items.Add(new Separator());

            contextMenu.Items.Add(new MenuItem()
            {
                Header = Resources.SID_Add_RTU,
                Command = new ContextMenuAction(_mapActions.AddNodeOnClick, _mapActions.CanAddNode),
                CommandParameter = EquipmentType.Rtu,
            });

            contextMenu.Items.Add(new Separator());

            contextMenu.Items.Add(new MenuItem()
            {
                Header = Resources.SID_Distace_measurement,
                Command = new ContextMenuAction(_mapActions.ToggleToDistanceMeasurementMode, _mapActions.CanMeasureDistance),
                CommandParameter = null,
            });

            return contextMenu;
        }
    }
}