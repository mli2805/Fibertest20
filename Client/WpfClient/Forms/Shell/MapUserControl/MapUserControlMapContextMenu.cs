using System.Windows;
using System.Windows.Controls;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    /// <summary>
    /// Context Menu for map itself is a resource in xaml
    /// 
    /// This is a handler
    /// </summary>
    public partial class MapUserControl
    {
        private void AddNodeOnClick(object sender, RoutedEventArgs e)
        {
            if (!(sender is MenuItem item)) return;

            var position = MainMap.FromLocalToLatLng(MainMap.ContextMenuPoint);
            var code = int.Parse((string)item.Tag);

            var equipmentType = (EquipmentType) code;
            var isAdjustmentNode = equipmentType == EquipmentType.AdjustmentNode;

            if (equipmentType == EquipmentType.Rtu)
                GraphReadModel.Request = new RequestAddRtuAtGpsLocation() { Latitude = position.Lat, Longitude = position.Lng };
            else if (equipmentType == EquipmentType.EmptyNode || equipmentType == EquipmentType.AdjustmentNode)
                GraphReadModel.Request = new AddNode()
                {
                    Latitude = position.Lat,
                    Longitude = position.Lng,
                    IsAdjustmentNode = isAdjustmentNode,
                };
            else
                GraphReadModel.Request = new RequestAddEquipmentAtGpsLocation()
                {
                    Type = (EquipmentType)code,
                    Latitude = position.Lat,
                    Longitude = position.Lng
                };
        }
    }

}
