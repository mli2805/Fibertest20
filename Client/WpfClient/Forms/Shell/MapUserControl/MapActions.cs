using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph.Requests;

namespace Iit.Fibertest.Client
{
    public class MapActions
    {
        private readonly GraphReadModel _graphReadModel;

        public MapActions(GraphReadModel graphReadModel)
        {
            _graphReadModel = graphReadModel;
        }

        public async void AddNodeOnClick(object param)
        {
            if (param is EquipmentType equipmentType)
                await AddNodeOnClick(equipmentType);
        }

        private async Task<string> AddNodeOnClick(EquipmentType equipmentType)
        {
            var position = _graphReadModel.MainMap.FromLocalToLatLng(_graphReadModel.MainMap.ContextMenuPoint);

            if (equipmentType == EquipmentType.Rtu)
                _graphReadModel.GrmRtuRequests.AddRtuAtGpsLocation(
                    new RequestAddRtuAtGpsLocation() { Latitude = position.Lat, Longitude = position.Lng });

            else
            {
                var expectedVisibilityLevel = equipmentType == EquipmentType.EmptyNode
                    ? GraphVisibilityLevel.EmptyNodes
                    : GraphVisibilityLevel.Equipments;
                if (_graphReadModel.SelectedGraphVisibilityItem.Level < expectedVisibilityLevel)
                    _graphReadModel.SetGraphVisibility(expectedVisibilityLevel);

                await _graphReadModel.GrmEquipmentRequests.AddEquipmentAtGpsLocation(
                    new RequestAddEquipmentAtGpsLocation()
                    {
                        Type = equipmentType,
                        Latitude = position.Lat,
                        Longitude = position.Lng
                    });
            }

            return null;
        }

        public void ToggleToDistanceMeasurementMode(object parameter)
        {
            if (!_graphReadModel.MainMap.IsInDistanceMeasurementMode)
            {
                _graphReadModel.CommonStatusBarViewModel.StatusBarMessage2 = StringResources.Resources.SID_Distance_measurement_mode;
                _graphReadModel.MainMap.IsInDistanceMeasurementMode = true;
                _graphReadModel.MainMap.StartNode = null;
            }
            else
            {
                _graphReadModel.MainMap.LeaveDistanceMeasurementMode();
                _graphReadModel.MainMap.IsInDistanceMeasurementMode = false;
                _graphReadModel.CommonStatusBarViewModel.StatusBarMessage2 = "";
            }
        }

        public bool CanAddNode(object parameter)
        {
            return _graphReadModel.CurrentUser.Role <= Role.Root;

        }


    }
}