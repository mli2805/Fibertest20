using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using GMap.NET;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class GpsInputViewModel : PropertyChangedBase
    {
        private readonly CurrentGis _currentGis;
        private readonly GraphReadModel _graphReadModel;
        private readonly Model _readModel;
        private readonly TabulatorViewModel _tabulatorViewModel;

        public OneCoorViewModel OneCoorViewModelLatitude { get; set; }
        public OneCoorViewModel OneCoorViewModelLongitude { get; set; }

        public PointLatLng Coors { get; set; }

        public List<GpsInputModeComboItem> GpsInputModes { get; set; } =
        (from mode in Enum.GetValues(typeof(GpsInputMode)).OfType<GpsInputMode>()
         select new GpsInputModeComboItem(mode)).ToList();

        private readonly GpsInputMode _modeInIniFile;
        private GpsInputModeComboItem _selectedGpsInputModeComboItem;
        public GpsInputModeComboItem SelectedGpsInputModeComboItem
        {
            get => _selectedGpsInputModeComboItem;
            set
            {
                if (Equals(value, _selectedGpsInputModeComboItem)) return;
                _selectedGpsInputModeComboItem = value;
                OneCoorViewModelLatitude.CurrentGpsInputMode = value.Mode;
                OneCoorViewModelLongitude.CurrentGpsInputMode = value.Mode;
                _currentGis.GpsInputMode = _selectedGpsInputModeComboItem.Mode;
            }
        }


        public bool IsEditEnabled { get; set; }
        private Guid _originalNodeId;
        public GpsInputViewModel(CurrentGis currentGis, GraphReadModel graphReadModel, 
            Model readModel, TabulatorViewModel tabulatorViewModel)
        {
            _currentGis = currentGis;
            _graphReadModel = graphReadModel;
            _readModel = readModel;
            _tabulatorViewModel = tabulatorViewModel;
            _modeInIniFile = currentGis.GpsInputMode;
            _selectedGpsInputModeComboItem = _modeInIniFile == GpsInputMode.Degrees
                ? new GpsInputModeComboItem(GpsInputMode.DegreesAndMinutes)
                : new GpsInputModeComboItem(GpsInputMode.Degrees);
        }

        public void Initialize(Node originalNode, bool isEditEnabled)
        {
            _originalNodeId = originalNode.NodeId;
            Coors = originalNode.Position;

            OneCoorViewModelLatitude = new OneCoorViewModel(SelectedGpsInputModeComboItem.Mode, Coors.Lat);
            OneCoorViewModelLongitude = new OneCoorViewModel(SelectedGpsInputModeComboItem.Mode, Coors.Lng);
            SelectedGpsInputModeComboItem = GpsInputModes.FirstOrDefault(i => i.Mode == _modeInIniFile);

            IsEditEnabled = isEditEnabled;
        }

        public string TryGetPoint(out PointLatLng point)
        {
            point = new PointLatLng();
            if (!OneCoorViewModelLatitude.TryGetValue(out double lat)) return OneCoorViewModelLatitude.Error;
            if (!OneCoorViewModelLongitude.TryGetValue(out double lng)) return OneCoorViewModelLongitude.Error;
            point = new PointLatLng(lat, lng);
            return null;
        }

        public async Task PreView()
        {
            var error = TryGetPoint(out PointLatLng position);
            if (error != null) return;

            if (!_tabulatorViewModel.IsGisOpen)
            {
                _tabulatorViewModel.OpenGis();
                await Task.Delay(100);

                if (_currentGis.ThresholdZoom > _graphReadModel.MainMap.Zoom)
                    _graphReadModel.MainMap.Zoom = _currentGis.ThresholdZoom;
            }

            _graphReadModel.ExtinguishAllNodes();

            var node = _readModel.Nodes.First(n => n.NodeId == _originalNodeId);
            node.Position = position;
            node.IsHighlighted = true;
            _graphReadModel.MainMap.SetPositionWithoutFiringEvent(position);
            await _graphReadModel.RefreshVisiblePart();

            var nodeVm = _graphReadModel.Data.Nodes.First(n => n.Id == _originalNodeId);
            nodeVm.Position = position;
            nodeVm.IsHighlighted = true;
        }

        public void DropChanges()
        {
            OneCoorViewModelLatitude.ReassignValue(Coors.Lat);
            OneCoorViewModelLongitude.ReassignValue(Coors.Lng);
        }

        public async Task ButtonDropChanges()
        {
            DropChanges();
            await PreView();
        }
    }
}
