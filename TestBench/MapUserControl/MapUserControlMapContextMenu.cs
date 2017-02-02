using System.Windows;
using System.Windows.Controls;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Commands;

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

            if (code == 99)
                GraphVm.Command = new AddMarker() { Latitude = position.Lat, Longitude = position.Lng};

            if ((EquipmentType)code == EquipmentType.Rtu)
                GraphVm.Command = new AddRtuAtGpsLocation() { Latitude = position.Lat, Longitude = position.Lng };
            else if ((EquipmentType)code == EquipmentType.Well || (EquipmentType)code == EquipmentType.Invisible)
                GraphVm.Command = new AddNode()
                {
                    Latitude = position.Lat,
                    Longitude = position.Lng,
                    IsJustForCurvature = (EquipmentType)code == EquipmentType.Invisible
                };
            else
                GraphVm.Command = new AddEquipmentAtGpsLocation()
                {
                    Type = (EquipmentType)code,
                    Latitude = position.Lat,
                    Longitude = position.Lng
                };
        }
    }

    public class AddMarker
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
