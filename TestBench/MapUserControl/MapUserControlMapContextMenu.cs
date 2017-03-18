using System.Windows;
using System.Windows.Controls;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.TestBench
{
    /// <summary>
    /// Context Menu for map itself is a resource in Xaml
    /// 
    /// This is a handler
    /// </summary>
    public partial class MapUserControl
    {
        private void AddNodeOnClick(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuItem;
            if (item == null) return;
            var code = int.Parse((string)item.Tag);
            var position = MainMap.FromLocalToLatLng(MainMap.ContextMenuPoint);

            if ((EquipmentType)code == EquipmentType.Rtu)
                GraphReadModel.Request = new RequestAddRtuAtGpsLocation() { Latitude = position.Lat, Longitude = position.Lng };
            else if ((EquipmentType)code == EquipmentType.Well || (EquipmentType)code == EquipmentType.Invisible)
                GraphReadModel.Request = new AddNode()
                {
                    Latitude = position.Lat,
                    Longitude = position.Lng,
                    IsJustForCurvature = (EquipmentType)code == EquipmentType.Invisible
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
