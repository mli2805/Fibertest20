using System.Windows;
using System.Windows.Controls;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Requests;

namespace Iit.Fibertest.Client
{
    /// <summary>
    /// Context Menu for map itself is a resource in xaml
    /// 
    /// This is a handler
    /// </summary>
    public partial class MapUserControl
    {
        private async void AddNodeOnClick(object sender, RoutedEventArgs e)
        {
            if (!(sender is MenuItem item)) return;

            var position = MainMap.FromLocalToLatLng(MainMap.ContextMenuPoint);
            var code = int.Parse((string)item.Tag);

            var equipmentType = (EquipmentType) code;

            if (equipmentType == EquipmentType.Rtu)
                GraphReadModel.GrmRtuRequests.AddRtuAtGpsLocation(
                    new RequestAddRtuAtGpsLocation() {Latitude = position.Lat, Longitude = position.Lng});

            else
                await GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation()
                {
                    Type = equipmentType,
                    Latitude = position.Lat,
                    Longitude = position.Lng
                });
        }
    }

}
