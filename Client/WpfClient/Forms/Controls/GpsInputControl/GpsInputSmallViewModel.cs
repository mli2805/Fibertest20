using System;
using Caliburn.Micro;
using GMap.NET;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class GpsInputSmallViewModel : PropertyChangedBase
    {
        private readonly CurrentGis _currentGis;
        private readonly Model _readModel;
        private readonly GraphReadModel _graphReadModel;
        private readonly TabulatorViewModel _tabulatorViewModel;

        private OneCoorViewModel _oneCoorViewModelLatitude;
        public OneCoorViewModel OneCoorViewModelLatitude
        {
            get => _oneCoorViewModelLatitude;
            set
            {
                if (Equals(value, _oneCoorViewModelLatitude)) return;
                _oneCoorViewModelLatitude = value;
                NotifyOfPropertyChange();
            }
        }

        private OneCoorViewModel _oneCoorViewModelLongitude;
        public OneCoorViewModel OneCoorViewModelLongitude
        {
            get => _oneCoorViewModelLongitude;
            set
            {
                if (Equals(value, _oneCoorViewModelLongitude)) return;
                _oneCoorViewModelLongitude = value;
                NotifyOfPropertyChange();
            }
        }

        public PointLatLng Coors { get; set; }
        private Guid _originalNodeId;

        public GpsInputSmallViewModel(CurrentGis currentGis,
            Model readModel, GraphReadModel graphReadModel, TabulatorViewModel tabulatorViewModel)
        {
            _currentGis = currentGis;
            _readModel = readModel;
            _graphReadModel = graphReadModel;
            _tabulatorViewModel = tabulatorViewModel;
            currentGis.PropertyChanged += CurrentGpsInputMode_PropertyChanged;
        }

        public void Initialize(PointLatLng coors, Guid originalNodeId)
        {
            Coors = coors;
            _originalNodeId = originalNodeId;

            OneCoorViewModelLatitude = new OneCoorViewModel(_currentGis.GpsInputMode, Coors.Lat);
            OneCoorViewModelLongitude = new OneCoorViewModel(_currentGis.GpsInputMode, Coors.Lng);
        }

        public async Task ShowPoint()
        {
            if (_tabulatorViewModel.SelectedTabIndex != 3)
            {
                _tabulatorViewModel.SelectedTabIndex = 3;
                await Task.Delay(100);

                if (_currentGis.ThresholdZoom > _graphReadModel.MainMap.Zoom)
                    _graphReadModel.MainMap.Zoom = _currentGis.ThresholdZoom;
            }

            _graphReadModel.ExtinguishAllNodes();

            var get = TryGetPoint(out PointLatLng position);
            if (get != null) return;

            var node = _readModel.Nodes.First(n => n.NodeId == _originalNodeId);
            node.Position = position;
            node.IsHighlighted = true;
            _graphReadModel.MainMap.SetPositionWithoutFiringEvent(position);
            await _graphReadModel.RefreshVisiblePart();

            var nodeVm = _graphReadModel.Data.Nodes.First(n => n.Id == _originalNodeId);
            nodeVm.Position = position;
            nodeVm.IsHighlighted = true;
        }

        public async Task CancelChanges()
        {
            OneCoorViewModelLatitude.ReassignValue(Coors.Lat);
            OneCoorViewModelLongitude.ReassignValue(Coors.Lng);
            await ShowPoint();
        }

        private void CurrentGpsInputMode_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OneCoorViewModelLatitude.CurrentGpsInputMode = ((CurrentGis)sender).GpsInputMode;
            OneCoorViewModelLongitude.CurrentGpsInputMode = ((CurrentGis)sender).GpsInputMode;
        }

        public string TryGetPoint(out PointLatLng point)
        {
            point = new PointLatLng();
            if (!OneCoorViewModelLatitude.TryGetValue(out double lat)) return OneCoorViewModelLatitude.Error;
            if (!OneCoorViewModelLongitude.TryGetValue(out double lng)) return OneCoorViewModelLongitude.Error;
            point = new PointLatLng(lat, lng);
            return null;
        }

    }
}
