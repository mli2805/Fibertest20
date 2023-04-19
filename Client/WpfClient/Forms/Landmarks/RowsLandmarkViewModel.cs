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
            return _changedLandmarks.First(l => l.NodeId == SelectedRow.NodeId);
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
            RefreshOnGpsInputModeChanged();
        }

        private bool _isFilterOn;
        public void ChangeFilterOn(bool isFilterOn)
        {
            _isFilterOn = isFilterOn;
            Rows = _changedLandmarks.LandmarksToRows(_changedLandmarks, _isFilterOn, _gpsInputMode);
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
        public async Task Initialize(Trace selectedTrace, Guid selectedNodeId)
        {
            _selectedTrace = selectedTrace;
            HasBaseRef = _selectedTrace.PreciseId != Guid.Empty;
            if (HasBaseRef)
                _sorData = await GetBase(_selectedTrace.PreciseId);

            var traceModel = _readModel.GetTraceComponentsByIds(_selectedTrace);
            _originalModel = TraceModelBuilder.GetTraceModelWithoutAdjustmentPoints(traceModel);
            _changedModel = _originalModel.Clone();

            Rows = BuildLandmarkRows();
            SelectedRow = Rows.First(r => r.NodeId == selectedNodeId);
        }

        // User changed one of landmarks and pressed Update table
        public void UpdateTable(Landmark changedLandmark)
        {
            if (SelectedRow == Rows.First())
                return; // It is disabled to edit RTU 

            var currentNode = _changedModel.NodeArray.First(n => n.NodeId == SelectedRow.NodeId);
            currentNode.UpdateFrom(changedLandmark);

            var indexOf = Array.IndexOf(_changedModel.NodeArray, currentNode);
            var currentFiber = _changedModel.FiberArray[indexOf - 1];
            currentFiber.UserInputedLength = changedLandmark.IsUserInput ? changedLandmark.UserInputLength : 0;

            _changedModel.EquipArray[indexOf].UpdateFrom(changedLandmark);

            Rows = ReCalculateLandmarks();
            SelectedRow = Rows.First(r => r.NodeId == SelectedRow.NodeId);
        }

        public void CancelChanges()
        {
            if (SelectedRow == Rows.First())
                return; // It is disabled to edit RTU 

            var currentNode = _changedModel.NodeArray.First(n => n.NodeId == SelectedRow.NodeId);
            var originalNode = _originalModel.NodeArray.First(n => n.NodeId == SelectedRow.NodeId);
            originalNode.CloneInto(currentNode);

            var indexOf = Array.IndexOf(_changedModel.NodeArray, currentNode);
            var currentFiber = _changedModel.FiberArray[indexOf - 1];
            currentFiber.UserInputedLength = _originalModel.FiberArray[indexOf - 1].UserInputedLength;

            var currentEquipment = _changedModel.EquipArray[indexOf];
            var originalEquipment = _originalModel.EquipArray[indexOf];
            originalEquipment.CloneInto(currentEquipment);

            Rows = ReCalculateLandmarks();
            SelectedRow = Rows.First(r => r.NodeId == SelectedRow.NodeId);
        }

        private void RefreshOnGpsInputModeChanged()
        {
            foreach (var row in Rows)
            {
                var landmark = _changedLandmarks.First(l => l.NodeId == row.NodeId);
                row.GpsCoors = landmark.GpsCoors.ToDetailedString(_gpsInputMode);
            }
        }

        private ObservableCollection<LandmarkRow> BuildLandmarkRows()
        {
            _changedLandmarks = HasBaseRef
                ? _landmarksBaseParser.GetLandmarks(_sorData, _changedModel)
                : _landmarksGraphParser.GetLandmarks(_selectedTrace);

            if (_originalLandmarks == null)
                _originalLandmarks = _changedLandmarks.Clone();

            return _changedLandmarks.LandmarksToRows(_originalLandmarks, _isFilterOn, _gpsInputMode);
        }

        private ObservableCollection<LandmarkRow> ReCalculateLandmarks()
        {
            if (HasBaseRef)
                _baseRefLandmarksTool.ReCalculateAndApplyProperties(_sorData, _changedModel);

            return BuildLandmarkRows();
        }

        private byte[] _sorBytes;
        private async Task<OtdrDataKnownBlocks> GetBase(Guid baseId)
        {
            var baseRef = _readModel.BaseRefs.First(b => b.Id == baseId);
            _preciseTimestamp = baseRef.SaveTimestamp;
            _sorBytes = await _c2DWcfCommonManager.GetSorBytes(baseRef.SorFileId);
            return SorData.FromBytes(_sorBytes);
        }

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

        public void CancelAllChangesForTrace()
        {
            _changedModel = _originalModel.Clone();
            Rows = ReCalculateLandmarks();
            SelectedRow = Rows.First(r => r.NodeId == SelectedRow.NodeId);
        }

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
