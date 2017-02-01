using System;
using System.Windows.Controls;
using Iit.Fibertest.Graph.Commands;

namespace Iit.Fibertest.TestBench
{
    public partial class MarkerControl
    {
        private bool CanUpdateRtu(object parameter) { return true; }

        private void AskUpdateRtue(object parameter)
        {
            var nodeId = (Guid)parameter;
            _owner.GraphVm.Command = new UpdateNode() { Id = nodeId };
        }
        private void OpenRtuContextMenu()
        {
            ContextMenu = new ContextMenu();
            ContextMenu.Items.Add(new MenuItem()
            {
                Header = "Информация Rtu",
                Command = new ContextMenuAction(AskUpdateRtue, CanUpdateRtu),
                CommandParameter = _marker.Id
            });
            ContextMenu.IsOpen = true;
        }
    }

}
/*        <ContextMenu x:Key="RtuContextMenu">
            <MenuItem Header="Информация" Tag="1" Click="MarkerContextMenuOnClick"/>
            <MenuItem Header="Ориентиры" Tag="3" Click="MarkerContextMenuOnClick"/>
            <MenuItem Header="Удалить РТУ" Tag="4" Click="MarkerContextMenuOnClick" IsEnabled="{Binding IsMenuItemRemoveNodeEnabled}"/>
            <Separator />
            <MenuItem Header="Участок" Tag="5" Click="MarkerContextMenuOnClick"/>
            <MenuItem Header="Участок с узламии" Tag="6" Click="MarkerContextMenuOnClick"/>
            <Separator />
            <MenuItem Header="Определить трассу" Tag="21" Click="MarkerContextMenuOnClick"/>
        </ContextMenu>
*/
