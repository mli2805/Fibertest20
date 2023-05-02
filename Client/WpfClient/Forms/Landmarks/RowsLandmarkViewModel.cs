using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WpfCommonViews;
using Optixsoft.SorExaminer.OtdrDataFormat;
using Landmark = Iit.Fibertest.Graph.Landmark;

namespace Iit.Fibertest.Client
{
    public class RowsLandmarkViewModel : PropertyChangedBase
    {
        private readonly ILifetimeScope _globalScope;
        private readonly Model _readModel;
        private readonly IWindowManager _windowManager;
        private readonly IWcfServiceCommonC2D _c2DWcfCommonManager;
        private readonly ReflectogramManager _reflectogramManager;
        private readonly LandmarksBaseParser _landmarksBaseParser;
        private readonly LandmarksGraphParser _landmarksGraphParser;
        private readonly BaseRefLandmarksTool _baseRefLandmarksTool;

        private Trace _selectedTrace;
        private DateTime _preciseTimestamp;

        private List<Landmark> _originalLandmarks;
        private List<Landmark> _changedLandmarks;
        public Landmark GetSelectedLandmark()
        {
            return _changedLandmarks.First(l => l.Number == SelectedRow.Number);
        }

        private OtdrDataKnownBlocks _sorData;
        private TraceModelForBaseRef _originalModel;
        private TraceModelForBaseRef _changedModel;

        private ObservableCollection<LandmarkRow> _rows;
        public ObservableCollection<LandmarkRow> Rows
        {
            get => _rows;
            set
            {
                if (Equals(value, _rows)) return;
                _rows = value;
                NotifyOfPropertyChange();
            }
        }

        private List<LandmarkRow> _originalLandmarkRows;
        private GpsInputMode _originalGpsInputMode;

        private LandmarkRow _selectedRow;
        public LandmarkRow SelectedRow
        {
            get => _selectedRow;
            set
            {
                if (value == null) return;
                _selectedRow = value;
                NotifyOfPropertyChange();
            }
        }

        private GpsInputMode _gpsInputMode;
        public void ChangeGpsInputMode(GpsInputMode gpsInputMode)
        {
            _gpsInputMode = gpsInputMode;
            Rows = _changedLandmarks.LandmarksToRows(_originalLandmarkRows, _isFilterOn, _gpsInputMode, _originalGpsInputMode);
        }

        private bool _isFilterOn;
        public void ChangeFilterOn(bool isFilterOn)
        {
            _isFilterOn = isFilterOn;
            Rows = _changedLandmarks.LandmarksToRows(_originalLandmarkRows, _isFilterOn, _gpsInputMode, _originalGpsInputMode);
        }

        public Visibility GisVisibility { get; set; }
        public int DataGridWidth { get; set; }

        private bool _hasBaseRef;
        public bool HasBaseRef
        {
            get => _hasBaseRef;
            set
            {
                if (value == _hasBaseRef) return;
                _hasBaseRef = value;
                NotifyOfPropertyChange();
            }
        }

        public readonly UpdateFromLandmarksBatch Command = new UpdateFromLandmarksBatch();

        public RowsLandmarkViewModel(ILifetimeScope globalScope, CurrentGis currentGis,
            Model readModel, IWindowManager windowManager,
            IWcfServiceCommonC2D c2DWcfCommonManager, ReflectogramManager reflectogramManager,
            LandmarksBaseParser landmarksBaseParser, LandmarksGraphParser landmarksGraphParser,
            BaseRefLandmarksTool baseRefLandmarksTool)
        {
            _globalScope = globalScope;
            _readModel = readModel;
            _windowManager = windowManager;
            _c2DWcfCommonManager = c2DWcfCommonManager;
            _reflectogramManager = reflectogramManager;
            _landmarksBaseParser = landmarksBaseParser;
            _landmarksGraphParser = landmarksGraphParser;
            _baseRefLandmarksTool = baseRefLandmarksTool;
            _gpsInputMode = currentGis.GpsInputMode;
            GisVisibility = currentGis.IsGisOn ? Visibility.Visible : Visibility.Collapsed;
            DataGridWidth = currentGis.IsGisOn ? 985 : 785;
        }

        // View just open or Trace changed
        // When it was click on Map - selectedNodeId is sent
        // in other variants number of row is sent
        public async Task Initialize(Trace selectedTrace, Guid selectedNodeId, int number = -1)
        {
            _selectedTrace = selectedTrace;
            HasBaseRef = _selectedTrace.PreciseId != Guid.Empty;
            if (HasBaseRef)
                _sorData = await GetBase(_selectedTrace.PreciseId);

            var traceModel = _readModel
                .GetTraceComponentsByIds(_selectedTrace)
                .ReCalculateGpsDistancesForTraceModel();

            // _originalModel is clone, so changes to ReadModel does not affects them
            _originalModel = traceModel.Clone();

            // contains references on Nodes/Fibers from ReadModel, so changes are "visible" on map
            _changedModel = traceModel.ExcludeAdjustmentPoints();

            _changedLandmarks = HasBaseRef
                ? _landmarksBaseParser.GetLandmarksFromBaseRef(_sorData, _selectedTrace)
                : _landmarksGraphParser.GetLandmarks(_selectedTrace);

            _originalLandmarks = _changedLandmarks.Clone();

            Rows = _changedLandmarks
                .LandmarksToRows(null, _isFilterOn, _gpsInputMode, _originalGpsInputMode);
            SelectedRow = number == -1 
                ? Rows.First(r => r.NodeId == selectedNodeId) 
                : Rows.First(r => r.Number == number);
            _originalLandmarkRows = Rows.ToList();
            _originalGpsInputMode = _gpsInputMode;
        }

        // User changed one of landmarks and pressed Update table
        public void UpdateTable(Landmark changedLandmark)
        {
            var originalLandmark = _originalLandmarks.First(l => l.Number == changedLandmark.Number);

            var currentNode = _changedModel.NodeArray.First(n => n.NodeId == SelectedRow.NodeId);
            var indexOf = Array.IndexOf(_changedModel.NodeArray, currentNode);

            if (originalLandmark.NodeTitle != changedLandmark.NodeTitle
                    || originalLandmark.NodeComment != changedLandmark.NodeComment
                    || !originalLandmark.GpsCoors.Equals(changedLandmark.GpsCoors))
            {
                currentNode.UpdateFrom(changedLandmark);
                Command.Add(currentNode);
            }

            if (!originalLandmark.UserInputLength.Equals(changedLandmark.UserInputLength))
            {
                var currentFiber = _changedModel.FiberArray[changedLandmark.Number - 1];
                currentFiber.UserInputedLength = changedLandmark.IsUserInput ? changedLandmark.UserInputLength : 0;
                Command.Add(currentFiber);
            }

            if (originalLandmark.EquipmentTitle != changedLandmark.EquipmentTitle
                || originalLandmark.EquipmentType != changedLandmark.EquipmentType
                || originalLandmark.LeftCableReserve != changedLandmark.LeftCableReserve
                || originalLandmark.RightCableReserve != changedLandmark.RightCableReserve)
            {
                var currentEquipment = _changedModel.EquipArray[changedLandmark.Number];
                currentEquipment.UpdateFrom(changedLandmark);
                Command.Add(currentEquipment);
            }

            Rows = ReCalculateLandmarks();
            SelectedRow = Rows.First(r => r.Number == SelectedRow.Number);
        }

        public void CancelChanges() // one landmarkRow
        {
            CancelChangesForRow(SelectedRow);

            Rows = ReCalculateLandmarks();
            SelectedRow = Rows.First(r => r.Number == SelectedRow.Number);
        }

        private void CancelChangesForRow(LandmarkRow landmarkRow)
        {
            var currentNode = _changedModel.NodeArray.First(n => n.NodeId == landmarkRow.NodeId);
            var originalNode = _originalModel.NodeArray.First(n => n.NodeId == landmarkRow.NodeId);
            originalNode.CloneInto(currentNode);
            Command.ClearNodeCommands(SelectedRow.NodeId);

            var currentFiber = _changedModel.FiberArray[landmarkRow.Number - 1];
            currentFiber.UserInputedLength = _originalModel.FiberArray[landmarkRow.Number - 1].UserInputedLength;
            Command.ClearFiberCommands(currentFiber.FiberId);

            var currentEquipment = _changedModel.EquipArray[landmarkRow.Number];
            // originalModel contains AdjustmentPoints, while changedModel does not
            // do not use IndexOf
            var originalEquipment = _originalModel.EquipArray
                .First(e => e.EquipmentId == currentEquipment.EquipmentId);
            originalEquipment.CloneInto(currentEquipment);
            Command.ClearEquipmentCommands(currentEquipment.EquipmentId);
        }

        // Update row, Cancel row, Cancel all rows
        private ObservableCollection<LandmarkRow> ReCalculateLandmarks()
        {
            // apply to SorData in order to recalculate Optical distances
            if (HasBaseRef)
                _baseRefLandmarksTool.ApplyTraceToBaseRef(_sorData, _selectedTrace, false);

            _changedLandmarks = HasBaseRef
                ? _landmarksBaseParser.GetLandmarksFromBaseRef(_sorData, _selectedTrace)
                : _landmarksGraphParser.GetLandmarks(_selectedTrace);

            return _changedLandmarks.LandmarksToRows(_originalLandmarkRows, _isFilterOn,
                _gpsInputMode, _originalGpsInputMode);
        }

        private byte[] _sorBytes;
        private async Task<OtdrDataKnownBlocks> GetBase(Guid baseId)
        {
            var baseRef = _readModel.BaseRefs.First(b => b.Id == baseId);
            _preciseTimestamp = baseRef.SaveTimestamp;
            _sorBytes = await _c2DWcfCommonManager.GetSorBytes(baseRef.SorFileId);
            return SorData.FromBytes(_sorBytes);
        }

        #region View's actions
        public void ShowReflectogram()
        {
            _reflectogramManager
                .ShowPreciseWithSelectedLandmark(_sorBytes, _selectedTrace.Title,
                    _preciseTimestamp, SelectedRow.Number + 1);
        }

        public void ExportToPdf()
        {
            var report = LandmarksReportProvider.Create(_changedLandmarks, _selectedTrace.Title, _gpsInputMode);
            PdfExposer.Show(report, $@"Landmarks {_selectedTrace.Title}.pdf", _windowManager);
        }

        // public void SetChangedAsNewOriginal()
        // {
        //     _originalModel = _changedModel.Clone();
        //     _originalLandmarks = _changedLandmarks.Clone();
        //
        //     Rows = ReCalculateLandmarks();
        //     SelectedRow = Rows.First(r => r.Number == SelectedRow.Number);
        //
        //     _originalLandmarkRows = Rows.ToList();
        // }
        //
        public void CancelAllChanges()
        {
            foreach (var landmarkRow in Rows.Skip(1))
                CancelChangesForRow(landmarkRow);

            Rows = ReCalculateLandmarks();
            SelectedRow = Rows.First(r => r.Number == SelectedRow.Number);
        }
        #endregion

        #region Rows context menu 
        public void ShowNode()
        {
            var vm = _globalScope.Resolve<NodeUpdateViewModel>();
            var node = _readModel.Nodes.First(n => n.NodeId == SelectedRow.NodeId);
            vm.Initialize(node.NodeId);
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public async void ShowFiber()
        {
            if (SelectedRow.FiberId == Guid.Empty) return;
            var vm = _globalScope.Resolve<FiberUpdateViewModel>();
            var fiber = _readModel.Fibers.First(f => f.FiberId == SelectedRow.FiberId);
            await vm.Initialize(fiber.FiberId);
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public void IncludeEquipment()
        {
            // будет через изменение оборудования на OneLandmarkViewModel
        }

        public void ExcludeEquipment()
        {
            // будет через изменение оборудования на OneLandmarkViewModel
        }
        #endregion
    }
}
