using System.Windows;
using System.Windows.Controls;
using Iit.Fibertest.Dto;
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
            {
                var expectedVisibilityLevel = equipmentType == EquipmentType.EmptyNode
                    ? GraphVisibilityLevel.EmptyNodes
                    : GraphVisibilityLevel.Equipments;
                if (GraphReadModel.SelectedGraphVisibilityItem.Level < expectedVisibilityLevel)
                {
                    GraphReadModel.SetGraphVisibility(expectedVisibilityLevel);
                }
                await GraphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(new RequestAddEquipmentAtGpsLocation()
                {
                    Type = equipmentType,
                    Latitude = position.Lat,
                    Longitude = position.Lng
                });
            }
        }

        private void ToggleToDistanceMeasurementMode(object sender, RoutedEventArgs e)
        {
            if (!MainMap.IsInDistanceMeasurementMode)
            {
                SetBanner(StringResources.Resources.SID_Distance_measurement_mode);
                MainMap.IsInDistanceMeasurementMode = true;
                MainMap.StartNode = null;
            }
        }
    }

}
