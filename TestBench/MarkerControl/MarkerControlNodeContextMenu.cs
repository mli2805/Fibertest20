using System;
using System.Windows.Controls;
using Iit.Fibertest.Graph.Commands;

namespace Iit.Fibertest.TestBench
{
    public partial class MarkerControl
    {
        private bool CanUpdateNode(object parameter) { return true; }

        private void AskUpdateNode(object parameter)
        {
            var nodeId = (Guid)parameter;

            _mainMap.
            Command = new UpdateNode() { Id = nodeId };
        }
        private void OpenNodeContextMenu()
        {
            ContextMenu = new ContextMenu();
            ContextMenu.Items.Add(new MenuItem()
            {
                Header = "Информация qqqqqqqqq",
                Command = new ContextMenuAction(AskUpdateNode, CanUpdateNode),
                CommandParameter = _marker.Id
            });
            ContextMenu.IsOpen = true;
        }
    }

}

/*
         private void MarkerContextMenuOnClick(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuItem;
            if (item == null) return;
            var code = int.Parse((string)item.Tag);

            switch (code)
            {
                case 4:
                    Command = new RemoveNode() { Id = _marker.Id };
                    return;
                case 5:
                    _mainMap.IsFiberWithNodes = false;
                    _mainMap.IsInFiberCreationMode = true;
                    _mainMap.StartNode = _marker;
                    Cursor = Cursors.Pen;
                    return;
                case 6:
                    _mainMap.IsFiberWithNodes = true;
                    _mainMap.IsInFiberCreationMode = true;
                    _mainMap.StartNode = _marker;
                    Cursor = Cursors.Pen;
                    return;
            }
        }

*/
/*
         <ContextMenu x:Key="NodeContextMenu">
            <MenuItem Header="Информация" Tag="1" Click="MarkerContextMenuOnClick"/>
            <MenuItem Header="Добавить оборудование" Tag="2" Click="MarkerContextMenuOnClick"/>
            <MenuItem Header="Ориентиры" Tag="3" Click="MarkerContextMenuOnClick"/>
            <MenuItem Header="Удалить узел" Tag="4" Click="MarkerContextMenuOnClick" IsEnabled="{Binding IsMenuItemRemoveNodeEnabled}"/>
            <Separator />
            <MenuItem Header="Участок" Tag="5" Click="MarkerContextMenuOnClick"/>
            <MenuItem Header="Участок с узламии" Tag="6" Click="MarkerContextMenuOnClick"/>
        </ContextMenu>
*/
