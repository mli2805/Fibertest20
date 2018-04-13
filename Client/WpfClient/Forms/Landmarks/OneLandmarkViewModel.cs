using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class OneLandmarkViewModel : PropertyChangedBase
    {
        public string TraceTitle;
        public DateTime PreciseTimestamp;
        public int SorFileId;

        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly GraphReadModel _graphReadModel;
        private readonly ReflectogramManager _reflectogramManager;

        public List<EquipmentTypeComboItem> ComboItems { get; set; } 
            = new List<EquipmentTypeComboItem>()
            {
                new EquipmentTypeComboItem(EquipmentType.EmptyNode),
                new EquipmentTypeComboItem(EquipmentType.CableReserve),
                new EquipmentTypeComboItem(EquipmentType.Closure),
                new EquipmentTypeComboItem(EquipmentType.Cross),
                new EquipmentTypeComboItem(EquipmentType.Other),
                new EquipmentTypeComboItem(EquipmentType.Terminal),
                new EquipmentTypeComboItem(EquipmentType.Rtu)
            };

        private EquipmentTypeComboItem _selectedEquipmentTypeItem;
        public EquipmentTypeComboItem SelectedEquipmentTypeItem
        {
            get => _selectedEquipmentTypeItem;
            set
            {
                if (Equals(value, _selectedEquipmentTypeItem)) return;
                _selectedEquipmentTypeItem = value;
                SelectedLandmark.EquipmentType = value.Type;
                NotifyOfPropertyChange();
            }
        }

        private GpsInputSmallViewModel _gpsInputSmallViewModel;
        public GpsInputSmallViewModel GpsInputSmallViewModel
        {
            get => _gpsInputSmallViewModel;
            set
            {
                if (Equals(value, _gpsInputSmallViewModel)) return;
                _gpsInputSmallViewModel = value;
                NotifyOfPropertyChange();
            }
        }

        private Landmark _landmarkBeforeChanges;

        private Landmark _selectedLandmark;
        public Landmark SelectedLandmark
        {
            get => _selectedLandmark;
            set
            {
              //  if (Equals(value, _selectedLandmark) || value == null) return;
                if (value == null) return;
                _selectedLandmark = value;
                _landmarkBeforeChanges = (Landmark) value.Clone();
                GpsInputSmallViewModel.Initialize(SelectedLandmark.GpsCoors);
                SelectedEquipmentTypeItem = ComboItems.First(i => i.Type == SelectedLandmark.EquipmentType);
                IsEquipmentEnabled = SelectedLandmark.EquipmentType != EquipmentType.EmptyNode &&
                                     SelectedLandmark.EquipmentType != EquipmentType.Rtu;
                NotifyOfPropertyChange();
            }
        }

        private bool _isEquipmentEnabled;

        public bool IsEquipmentEnabled
        {
            get => _isEquipmentEnabled;
            set
            {
                if (value == _isEquipmentEnabled) return;
                _isEquipmentEnabled = value;
                NotifyOfPropertyChange();
            }
        }


        public OneLandmarkViewModel(GpsInputSmallViewModel gpsInputSmallViewModel, IWcfServiceForClient c2DWcfManager,
            GraphReadModel graphReadModel,  ReflectogramManager reflectogramManager)
        {
            _c2DWcfManager = c2DWcfManager;
            _graphReadModel = graphReadModel;
            _reflectogramManager = reflectogramManager;
            GpsInputSmallViewModel = gpsInputSmallViewModel;
        }

        public async void Apply()
        {
            await ApplyEquipment();
            await ApplyNode();
        }

        private async Task<string> ApplyEquipment()
        {
            if (_landmarkBeforeChanges.EquipmentTitle != SelectedLandmark.EquipmentTitle ||
                _landmarkBeforeChanges.EquipmentType != SelectedLandmark.EquipmentType)
            {
                return await _c2DWcfManager.SendCommandAsObj(
                    new UpdateEquipment{EquipmentId = SelectedLandmark.EquipmentId,
                        Title = SelectedLandmark.EquipmentTitle, Type = SelectedLandmark.EquipmentType});
            }
            return null;
        }

        private async Task<string> ApplyNode()
        {
            if (_landmarkBeforeChanges.NodeTitle != SelectedLandmark.NodeTitle ||
                _landmarkBeforeChanges.NodeComment != SelectedLandmark.NodeComment ||
                _landmarkBeforeChanges.GpsCoors != GpsInputSmallViewModel.Get())
            {
                return await _c2DWcfManager.SendCommandAsObj(
                    new UpdateAndMoveNode{NodeId = SelectedLandmark.NodeId, Title = SelectedLandmark.NodeTitle,
                        Comment = SelectedLandmark.NodeComment, GpsCoors = GpsInputSmallViewModel.Get()});
            }
            _graphReadModel.Extinguish();
            return null;
        }

        public void Cancel()
        {
            if (_landmarkBeforeChanges == null) return;

            SelectedLandmark = _landmarkBeforeChanges;
            var nodeVm = _graphReadModel.Data.Nodes.First(n => n.Id == SelectedLandmark.NodeId);
            nodeVm.Position = GpsInputSmallViewModel.Get();
            _graphReadModel.Extinguish();
        }

        public void ShowLandmarkOnMap()
        {
            _graphReadModel.Extinguish();
            var nodeVm = _graphReadModel.Data.Nodes.First(n => n.Id == SelectedLandmark.NodeId);
            nodeVm.Position = GpsInputSmallViewModel.Get();
            _graphReadModel.PlaceNodeIntoScreenCenter(SelectedLandmark.NodeId);
        }
        public void ShowReflectogram()
        {
            _reflectogramManager.SetTempFileName(TraceTitle, BaseRefType.Precise.ToString(), PreciseTimestamp);
            _reflectogramManager.ShowBaseReflectogramWithSelectedLandmark(SorFileId, SelectedLandmark.Number);
        }
    }
}
