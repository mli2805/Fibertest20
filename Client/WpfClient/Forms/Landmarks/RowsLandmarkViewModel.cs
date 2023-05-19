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

        // нужны для определения что изменилось,
        // использовать _originalLandmarkRows нельзя потому что там нет Node.Comment
        private List<Landmark> _originalLandmarks;
        private List<Landmark> _changedLandmarks;

        private OtdrDataKnownBlocks _sorData;
        // нужна для восстановления значений при нажатии Отменить
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

        // нужна только для отрисовки желтым изменившихся полей
        private List<LandmarkRow> _originalLandmarkRows;
        private GpsInputMode _originalGpsInputMode;

        private LandmarkRow _selectedRow;
        public LandmarkRow SelectedRow
        {
            get => _selectedRow;
            set => ChangeSelectedRow(value);
        }

        private async void ChangeSelectedRow(LandmarkRow value)
        {
            if (value == null) return;
            _selectedRow = value;
            await OneLandmarkViewModel.GpsInputSmallViewModel.CancelChanges();
            OneLandmarkViewModel.Initialize(
                _changedLandmarks.First(l => l.Number == SelectedRow.Number));
            await OneLandmarkViewModel.GpsInputSmallViewModel.ShowPoint();
            NotifyOfPropertyChange();
            NotifyOfPropertyChange(nameof(IsEquipmentOpEnabled));
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

        private GpsInputMode _gpsInputMode;
        public void ChangeGpsInputMode(GpsInputMode gpsInputMode)
        {
            _gpsInputMode = gpsInputMode;
            Rows = _changedLandmarks.LandmarksToRows(_originalLandmarkRows, _isFilterOn, _gpsInputMode, _originalGpsInputMode);
            SelectedRow = Rows.First(r => r.Number == _selectedRow.Number);
        }

        private bool _isFilterOn;
        public void ChangeFilterOn(bool isFilterOn)
        {
            _isFilterOn = isFilterOn;
            Rows = _changedLandmarks.LandmarksToRows(_originalLandmarkRows, _isFilterOn, _gpsInputMode, _originalGpsInputMode);
            SelectedRow = Rows.First(r => r.Number == _selectedRow.Number);
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
                NotifyOfPropertyChange(nameof(IsEquipmentOpEnabled));
            }
        }

        public bool IsEquipmentOpEnabled => !HasBaseRef && SelectedRow.Number != 0;

        public readonly UpdateFromLandmarksBatch Command = new UpdateFromLandmarksBatch();
        public bool AreThereAnyChanges => Command.Any();

        public RowsLandmarkViewModel(ILifetimeScope globalScope, CurrentGis currentGis,
            Model readModel, IWindowManager windowManager,
            IWcfServiceCommonC2D c2DWcfCommonManager, ReflectogramManager reflectogramManager,
            LandmarksBaseParser landmarksBaseParser, LandmarksGraphParser landmarksGraphParser,
            BaseRefLandmarksTool baseRefLandmarksTool, OneLandmarkViewModel oneLandmarkViewModel)
        {
            _globalScope = globalScope;
            _readModel = readModel;
            _windowManager = windowManager;
            _c2DWcfCommonManager = c2DWcfCommonManager;
            _reflectogramManager = reflectogramManager;
            _landmarksBaseParser = landmarksBaseParser;
            _landmarksGraphParser = landmarksGraphParser;
            _baseRefLandmarksTool = baseRefLandmarksTool;

            OneLandmarkViewModel = oneLandmarkViewModel;

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
                .GetTraceComponentsByIds(_selectedTrace);

            // _originalModel is а clone, so changes to ReadModel does not affects them
            _originalModel = traceModel.Clone();

            // ++++ contains references on Nodes/Fibers from ReadModel, so changes are "visible" on map
            // ++++ and GetLandmarksFromBaseRef get GPS distances from traceModel, now traceModel contains Adjustment points
            _changedModel = traceModel;

            _changedLandmarks = HasBaseRef
                ? _landmarksBaseParser.GetLandmarksFromBaseRef(_sorData, _changedModel)
                : _landmarksGraphParser.GetLandmarksFromGraph(_selectedTrace);

            _originalLandmarks = _changedLandmarks.Clone();

            Rows = _changedLandmarks
                .LandmarksToRows(null, _isFilterOn, _gpsInputMode, _originalGpsInputMode);
            _selectedRow = number == -1
                ? Rows.First(r => r.NodeId == selectedNodeId)
                : Rows.First(r => r.Number == number);
            _originalLandmarkRows = Rows.ToList();
            _originalGpsInputMode = _gpsInputMode;

            OneLandmarkViewModel.Initialize(_changedLandmarks.First(l=>l.Number == _selectedRow.Number));
        }

        // Ландмарков меньше чем узлов в модели (из-за точек привязки)
        public void UpdateTable()
        {
            Landmark changedLandmark = OneLandmarkViewModel.GetLandmark();
            var originalLandmark = _originalLandmarks.First(l => l.Number == changedLandmark.Number);

            var currentNode = _changedModel.NodeArray.First(n => n.NodeId == SelectedRow.NodeId);

            if (originalLandmark.NodePropertiesChanged(changedLandmark))
            {
                currentNode.UpdateFrom(changedLandmark);
                Command.Add(currentNode);
                NotifyOfPropertyChange(nameof(AreThereAnyChanges));
            }

            if (!originalLandmark.UserInputLength.Equals(changedLandmark.UserInputLength))
            {
                var currentFiber = _changedModel.FiberArray[changedLandmark.NumberIncludingAdjustmentPoints - 1];
                var allParts = _readModel.GetAllParts(currentFiber.FiberId);
                foreach (var fiberPartId in allParts)
                {
                    var fiberPart = _readModel.Fibers.First(f => f.FiberId == fiberPartId);
                    fiberPart.UserInputedLength = changedLandmark.IsUserInput ? changedLandmark.UserInputLength : 0;
                }
                Command.Add(currentFiber);
                NotifyOfPropertyChange(nameof(AreThereAnyChanges));
            }

            if (originalLandmark.EquipmentPropertiesChanged(changedLandmark))
            {
                var currentEquipment = _changedModel.EquipArray[changedLandmark.NumberIncludingAdjustmentPoints];
                currentEquipment.UpdateFrom(changedLandmark);
                Command.Add(currentEquipment);
                NotifyOfPropertyChange(nameof(AreThereAnyChanges));
            }

            Rows = ReCalculateLandmarks();
            SelectedRow = Rows.First(r => r.Number == SelectedRow.Number);
        }

        public void CancelChanges() // one landmarkRow
        {
            CancelChangesForRow(SelectedRow);

            NotifyOfPropertyChange(nameof(AreThereAnyChanges));
            Rows = ReCalculateLandmarks();
            SelectedRow = Rows.First(r => r.Number == SelectedRow.Number);
        }

        // traceModels contain AdjustmentPoints, while Rows does not
        private void CancelChangesForRow(LandmarkRow landmarkRow)
        {
            var currentNode = _changedModel.NodeArray.First(n => n.NodeId == landmarkRow.NodeId);
            var originalNode = _originalModel.NodeArray.First(n => n.NodeId == landmarkRow.NodeId);
            originalNode.CloneInto(currentNode);
            Command.ClearNodeCommands(landmarkRow.NodeId);

            var currentFiber = _changedModel.FiberArray[landmarkRow.NumberIncludingAdjustmentPoints - 1];

            var allParts = _readModel.GetAllParts(currentFiber.FiberId);
            foreach (var fiberPartId in allParts)
            {
                var fiberPart = _readModel.Fibers.First(f => f.FiberId == fiberPartId);
                fiberPart.UserInputedLength = _originalModel.FiberArray
                    .First(f => f.FiberId == currentFiber.FiberId).UserInputedLength;
            }
            Command.ClearFiberCommands(currentFiber.FiberId);

            var currentEquipment = _changedModel.EquipArray[landmarkRow.NumberIncludingAdjustmentPoints];
            var originalEquipment = _originalModel.EquipArray[landmarkRow.NumberIncludingAdjustmentPoints];
            if (currentEquipment.EquipmentId != originalEquipment.EquipmentId)
            {
                var eq = _readModel.Equipments.First(e => e.EquipmentId == originalEquipment.EquipmentId);
                _changedModel.EquipArray[landmarkRow.NumberIncludingAdjustmentPoints] = eq;
                Command.ClearReplaceCommands(landmarkRow.NumberIncludingAdjustmentPoints);
            }
            // эти строки выполняются без условно -
            // если перед заменой оборудования старое было как-то изменено - оно восстанавливается
            currentEquipment = _changedModel.EquipArray[landmarkRow.NumberIncludingAdjustmentPoints];
            originalEquipment.CloneInto(currentEquipment);
            Command.ClearEquipmentCommands(currentEquipment.EquipmentId);
        }

        // Update row, Cancel row, Cancel all rows
        private ObservableCollection<LandmarkRow> ReCalculateLandmarks()
        {
            // из-за изменения координат и пользовательских длин меняется колонка длина по GPS,
            // надо не собирать всю модель заново, а только расстояния пересчитать
            _changedModel = _changedModel.FillInGpsDistancesForTraceModel();

            // если есть сорка передвинуть ориентиры в сорке
            if (HasBaseRef)
            {
                var withoutAdjustmentPoints = _changedModel.ExcludeAdjustmentPoints();
                _baseRefLandmarksTool.ReCalculateLandmarksLocations(_sorData, withoutAdjustmentPoints);
            }

            // заново извлечь ориентиры из изменившейся модели
            _changedLandmarks = HasBaseRef
                ? _landmarksBaseParser.GetLandmarksFromBaseRef(_sorData, _changedModel)
                : _landmarksGraphParser.GetLandmarksFromModel(_changedModel);

            return _changedLandmarks.LandmarksToRows(_originalLandmarkRows, _isFilterOn,
                _gpsInputMode, _originalGpsInputMode);
        }

        private async Task<OtdrDataKnownBlocks> GetBase(Guid baseId)
        {
            var baseRef = _readModel.BaseRefs.First(b => b.Id == baseId);
            _preciseTimestamp = baseRef.SaveTimestamp;
            var sorBytes = await _c2DWcfCommonManager.GetSorBytes(baseRef.SorFileId);
            return SorData.FromBytes(sorBytes);
        }

        #region View's actions
        public void ShowReflectogram()
        {
            _reflectogramManager
                .ShowPreciseWithSelectedLandmark(_sorData.ToBytes(), _selectedTrace.Title,
                    _preciseTimestamp, SelectedRow.Number + 1);
        }

        public void ExportToPdf()
        {
            var report = LandmarksReportProvider.Create(_changedLandmarks, _selectedTrace.Title, _gpsInputMode);
            PdfExposer.Show(report, $@"Landmarks {_selectedTrace.Title}.pdf", _windowManager);
        }

        public void CancelAllChanges()
        {
            foreach (var landmarkRow in Rows.Skip(1))
                CancelChangesForRow(landmarkRow);

            NotifyOfPropertyChange(nameof(AreThereAnyChanges));
            Rows = ReCalculateLandmarks();
            SelectedRow = Rows.First(r => r.Number == SelectedRow.Number);
        }
        #endregion

        #region Rows context menu 
        public void ShowNode()
        {
            var vm = _globalScope.Resolve<NodeUpdateViewModel>();
            var node = _readModel.Nodes.First(n => n.NodeId == SelectedRow.NodeId);
            vm.InitializeFromLandmarksView(node.NodeId, _changedModel);
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public async void ShowFiber()
        {
            if (SelectedRow.FiberId == Guid.Empty) return;
            var vm = _globalScope.Resolve<FiberUpdateViewModel>();
            var fiber = _readModel.Fibers.First(f => f.FiberId == SelectedRow.FiberId);
            await vm.Initialize(fiber.FiberId, true);
            _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public void ChangeEquipment()
        {
            var node = _readModel.Nodes.First(n => n.NodeId == SelectedRow.NodeId);
            var allEquipmentInNode = _readModel.Equipments.Where(e => e.NodeId == node.NodeId).ToList();
            var traceContentChoiceViewModel = _globalScope.Resolve<TraceContentChoiceViewModel>();
            traceContentChoiceViewModel.Initialize(allEquipmentInNode, node, false, false);
            _windowManager.ShowDialogWithAssignedOwner(traceContentChoiceViewModel);
            var selectedEquipmentGuid = traceContentChoiceViewModel.GetSelectedEquipmentGuid();
            if (!traceContentChoiceViewModel.ShouldWeContinue ||
                selectedEquipmentGuid == SelectedRow.EquipmentId) return;

            // включить в трассу выбранное оборудование
            // или исключить - в этом случае selectedEquipmentGuid это id "пустого" оборудования в этом узле
            // команда ExcludeEquipmentFromTrace существует только для обратной совместимости
            var cmd = new IncludeEquipmentIntoTrace()
            {
                TraceId = _selectedTrace.TraceId,
                IndexInTrace = SelectedRow.NumberIncludingAdjustmentPoints,
                EquipmentId = selectedEquipmentGuid,
            };

            Command.Add(cmd);
            NotifyOfPropertyChange(nameof(AreThereAnyChanges));

            var newEquipment = _readModel.Equipments.FirstOrDefault(e => e.EquipmentId == selectedEquipmentGuid);
            _changedModel.EquipArray[SelectedRow.NumberIncludingAdjustmentPoints] = newEquipment;

            Rows = ReCalculateLandmarks();
            SelectedRow = Rows.First(r => r.Number == SelectedRow.Number);
        }
        #endregion
    }
}
