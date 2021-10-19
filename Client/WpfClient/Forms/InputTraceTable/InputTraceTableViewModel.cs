using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Autofac;
using Caliburn.Micro;
using GMap.NET;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class InputTraceTableViewModel : Screen
    {
        private readonly ILifetimeScope _globalScope;
        private readonly Model _readModel;
        private RtuLeaf _rtuLeaf;
        private Rtu _rtu;


        #region Rows

        private ObservableCollection<TraceTableRow> _rows;
        public ObservableCollection<TraceTableRow> Rows
        {
            get => _rows;
            set
            {
                if (Equals(value, _rows)) return;
                _rows = value;
                NotifyOfPropertyChange();
            }
        }

        private TraceTableRow _selectedRow;
        public TraceTableRow SelectedRow
        {
            get => _selectedRow;
            set
            {
                if (Equals(value, _selectedRow)) return;
                _selectedRow = value;
                NotifyOfPropertyChange();
                if (_selectedRow != null)
                    InitiateOneLandmarkControl();
                NotifyOfPropertyChange(nameof(IsNotRtuSelected));
            }
        }

        private OneLandmarkViewModel _oneLandmarkViewModel;
        public OneLandmarkViewModel OneLandmarkViewModel
        {
            get => _oneLandmarkViewModel;
            set
            {
                if (Equals(value, _oneLandmarkViewModel)) return;
                _oneLandmarkViewModel = value;
                NotifyOfPropertyChange();
            }
        }

        private void InitiateOneLandmarkControl()
        {
            OneLandmarkViewModel.Cancel();
            OneLandmarkViewModel.SelectedLandmark = new Landmark()
            {
                NodeTitle = SelectedRow.NodeTitle,
                EquipmentType = SelectedRow.EquipmentType == @"RTU" 
                    ? EquipmentType.Rtu 
                    : (EquipmentType)Enum.Parse(typeof(EquipmentType), SelectedRow.EquipmentType),
                EquipmentTitle = SelectedRow.EquipmentTitle,
                // GpsCoors = SelectedRow.GpsCoors,
                GpsCoors = new PointLatLng(53.1425, 30.3456),
            };
            OneLandmarkViewModel.IsFromBaseRef = false;
        }

        #endregion

        #region Gis

        public List<GpsInputModeComboItem> GpsInputModes { get; set; } =
            (from mode in Enum.GetValues(typeof(GpsInputMode)).OfType<GpsInputMode>()
             select new GpsInputModeComboItem(mode)).ToList();
        public CurrentGis CurrentGis { get; }
        private GpsInputModeComboItem _selectedGpsInputMode;
        public GpsInputModeComboItem SelectedGpsInputMode
        {
            get => _selectedGpsInputMode;
            set
            {
                if (Equals(value, _selectedGpsInputMode)) return;
                _selectedGpsInputMode = value;
                NotifyOfPropertyChange();
            }
        }
        public Visibility GisVisibility { get; set; }

        #endregion

        public bool IsRowCopied { get; set; }
        public bool IsNotRtuSelected => SelectedRow == Rows.First();

       
        public int DataGridWidth { get; set; }

        public InputTraceTableViewModel(ILifetimeScope globalScope, CurrentGis currentGis, Model readModel)
        {
            _globalScope = globalScope;
            _readModel = readModel;
            CurrentGis = currentGis;
            _selectedGpsInputMode = GpsInputModes.First(i => i.Mode == CurrentGis.GpsInputMode);
            GisVisibility = currentGis.IsGisOn ? Visibility.Visible : Visibility.Collapsed;
            DataGridWidth = currentGis.IsGisOn ? 590 : 390;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = $@"Input trace  (RTU:  {_rtuLeaf.Title})";
        }

        public void Initialize(RtuLeaf rtuLeaf)
        {
            _rtuLeaf = rtuLeaf;
            _rtu = _readModel.Rtus.First(r => r.Id == _rtuLeaf.Id);
            var node = _readModel.Nodes.First(n => n.NodeId == _rtu.NodeId);
            OneLandmarkViewModel = _globalScope.Resolve<OneLandmarkViewModel>();
            Rows = new ObservableCollection<TraceTableRow>();
            Rows.Add(new TraceTableRow()
            {
                Ordinal = 1,
                NodeTitle = _rtuLeaf.Title,
                EquipmentType = @"RTU",
                GpsCoors = CurrentGis.IsGisOn ? node.Position.ToDetailedString(CurrentGis.GpsInputMode) : "",
            });
            SelectedRow = Rows.First();

            // OneLandmarkViewModel.RtuId = _rtuLeaf.Id;
            // OneLandmarkViewModel.SelectedLandmark = new Landmark()
            // {
            //     NodeTitle = _rtuLeaf.Title,
            //     EquipmentType = EquipmentType.Rtu,
            //     GpsCoors = node.Position,
            //     NodeComment = _rtu.Comment,
            // };
        }

        #region DataGrid Items Menu

        private TraceTableRow _rowInClipboard;

        public void AddNewRowAfterSelected()
        {
            var index = Rows.IndexOf(SelectedRow);
            Rows.Insert(index + 1, new TraceTableRow());
            ReNumerateRows();
        }

        public void CutSelectedRow()
        {
            IsRowCopied = true;
            _rowInClipboard = SelectedRow.Clone();
            var index = Rows.IndexOf(SelectedRow);
            Rows.Remove(SelectedRow);
            if (Rows.Count <= index)
                index--;
            SelectedRow = Rows[index];
            ReNumerateRows();
        }

        public void CopySelectedRow()
        {
            IsRowCopied = true;
            _rowInClipboard = SelectedRow.Clone();
            ReNumerateRows();
        }

        public void PasteRowAfterSelected()
        {
            var index = Rows.IndexOf(SelectedRow);
            Rows.Insert(index + 1, _rowInClipboard);
            ReNumerateRows();
        }

        private void ReNumerateRows()
        {
            var number = 1;
            foreach (var traceTableRow in Rows)
            {
                traceTableRow.Ordinal = number++;
            }
        }

        #endregion

        #region Buttons



        #endregion
    }
}
