using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Caliburn.Micro;
using GMap.NET;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WpfCommonViews;
using Trace = Iit.Fibertest.Graph.Trace;

namespace Iit.Fibertest.Client
{
    public class LandmarksViewModel : Screen
    {
        private Rtu _rtu;
        public CurrentGis CurrentGis { get; }
        public List<GpsInputModeComboItem> GpsInputModes { get; set; } =
            (from mode in Enum.GetValues(typeof(GpsInputMode)).OfType<GpsInputMode>()
             select new GpsInputModeComboItem(mode)).ToList();

        public List<Trace> Traces { get; set; }

        private Trace _selectedTrace;
        public Trace SelectedTrace
        {
            get => _selectedTrace;
            set
            {
                if (Equals(value, _selectedTrace)) return;
                _selectedTrace = value;
#pragma warning disable CS4014
                RefreshOnChangedTrace();
#pragma warning restore CS4014
            }
        }

        private GpsInputModeComboItem _selectedGpsInputMode;
        public GpsInputModeComboItem SelectedGpsInputMode
        {
            get => _selectedGpsInputMode;
            set
            {
                if (Equals(value, _selectedGpsInputMode)) return;
                _selectedGpsInputMode = value;
                CurrentGis.GpsInputMode = _selectedGpsInputMode.Mode;
                RowsLandmarkViewModel.ChangeGpsInputMode(_selectedGpsInputMode.Mode);
            }
        }

        private bool _isFilterOn;
        public bool IsFilterOn
        {
            get => _isFilterOn;
            set
            {
                if (value == _isFilterOn) return;
                _isFilterOn = value;
                RowsLandmarkViewModel.ChangeFilterOn(_isFilterOn);
            }
        }

        private readonly ILifetimeScope _globalScope;
        private readonly Model _readModel;
        private readonly IWindowManager _windowManager;
        private readonly GraphReadModel _graphReadModel;
        private readonly TabulatorViewModel _tabulatorViewModel;

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

        private RowsLandmarkViewModel _rowsLandmarkViewModel;

        public RowsLandmarkViewModel RowsLandmarkViewModel
        {
            get => _rowsLandmarkViewModel;
            set
            {
                if (Equals(value, _rowsLandmarkViewModel)) return;
                _rowsLandmarkViewModel = value;
                NotifyOfPropertyChange();
            }
        }

        public Visibility GisVisibility { get; set; }

        public LandmarksViewModel(ILifetimeScope globalScope, Model readModel, 
            CurrentGis currentGis, IWindowManager windowManager,
            RowsLandmarkViewModel rowsLandmarkViewModel, OneLandmarkViewModel oneLandmarkViewModel,
            GraphReadModel graphReadModel, TabulatorViewModel tabulatorViewModel)
        {
            CurrentGis = currentGis;
            _globalScope = globalScope;
            _readModel = readModel;
            _windowManager = windowManager;
            _graphReadModel = graphReadModel;
            _tabulatorViewModel = tabulatorViewModel;
            _selectedGpsInputMode = GpsInputModes.First(i => i.Mode == CurrentGis.GpsInputMode);
            GisVisibility = currentGis.IsGisOn ? Visibility.Visible : Visibility.Collapsed;

            RowsLandmarkViewModel = rowsLandmarkViewModel;
            RowsLandmarkViewModel.PropertyChanged += RowsLandmarkViewModel_PropertyChanged;
            OneLandmarkViewModel = oneLandmarkViewModel;
        }

        public async Task InitializeFromRtu(Guid rtuId)
        {
            _rtu = _readModel.Rtus.First(r => r.Id == rtuId);
            Traces = _readModel.Traces.Where(t => t.RtuId == rtuId).ToList();
            if (Traces.Count == 0) return;
            _selectedTrace = Traces.First();

            await RowsLandmarkViewModel.Initialize(SelectedTrace, Guid.Empty, 0);
        }

        public async Task InitializeFromTrace(Guid traceId, Guid selectedNodeId)
        {
            var trace = _readModel.Traces.First(t => t.TraceId == traceId);
            _rtu = _readModel.Rtus.First(r => r.Id == trace.RtuId);
            Traces = _readModel.Traces.Where(t => t.RtuId == trace.RtuId).ToList();
            _selectedTrace = _readModel.Traces.First(t => t.TraceId == traceId);
            await RowsLandmarkViewModel.Initialize(SelectedTrace, selectedNodeId);
            OneLandmarkViewModel.Initialize(RowsLandmarkViewModel.GetSelectedLandmark());
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Landmarks_of_traces_of_RTU_ + _rtu.Title;
        }

        public async Task RefreshOnChangedTrace()
        {
            await RowsLandmarkViewModel.Initialize(SelectedTrace, Guid.Empty, 0);
            OneLandmarkViewModel.Initialize(RowsLandmarkViewModel.GetSelectedLandmark());
        }

        private void RowsLandmarkViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == @"SelectedRow")
            {
                OneLandmarkViewModel.Initialize(RowsLandmarkViewModel.GetSelectedLandmark());
            }
        }

        #region Whole trace buttons
        private bool _areThereAnyChanges;
        public bool AreThereAnyChanges
        {
            get => _areThereAnyChanges;
            set
            {
                if (value == _areThereAnyChanges) return;
                _areThereAnyChanges = value;
                NotifyOfPropertyChange();
            }
        }

        public async Task SaveAllChanges()
        {
            var dto = RowsLandmarkViewModel.Command.BuildDto();
            var vm = _globalScope.Resolve<LandmarksCorrectionProgressViewModel>();
            vm.Initialize(dto);
            _windowManager.ShowDialogWithAssignedOwner(vm);
            if (!vm.IsSentSuccessfully) return;

            RowsLandmarkViewModel.Command.ClearAll();

            await RowsLandmarkViewModel
                .Initialize(_selectedTrace, Guid.Empty, RowsLandmarkViewModel.SelectedRow.Number);
            AreThereAnyChanges = RowsLandmarkViewModel.Command.Any();
        }

        public void CancelAllChanges()
        {
            RowsLandmarkViewModel.CancelAllChanges();
            AreThereAnyChanges = RowsLandmarkViewModel.Command.Any();
        }

        public void ExportToPdf()
        {
            RowsLandmarkViewModel.ExportToPdf();
        }

        public void ShowReflectogram()
        {
            RowsLandmarkViewModel.ShowReflectogram();
        }
        #endregion


        #region One landmark buttons
        public void UpdateTable() // Button, fill in changes in selected landmark and re-calculate all of them
        {
            OneLandmarkViewModel.IsEditEnabled = false;
            _graphReadModel.ExtinguishAllNodes();
            RowsLandmarkViewModel.UpdateTable(OneLandmarkViewModel.GetLandmark());
            OneLandmarkViewModel.IsEditEnabled = true;
            AreThereAnyChanges = RowsLandmarkViewModel.Command.Any();
        }

        public void CancelChanges()
        {
            RowsLandmarkViewModel.CancelChanges();
            AreThereAnyChanges = RowsLandmarkViewModel.Command.Any();
        }

        public async void ShowLandmarkOnMap()
        {
            await ShowLandmark();
        }

        private async Task ShowLandmark()
        {
            if (_tabulatorViewModel.SelectedTabIndex != 3)
            {
                _tabulatorViewModel.SelectedTabIndex = 3;
                await Task.Delay(100);
            }

            if (CurrentGis.ThresholdZoom > _graphReadModel.MainMap.Zoom)
                _graphReadModel.MainMap.Zoom = CurrentGis.ThresholdZoom;
            _graphReadModel.ExtinguishAllNodes();

            var node = _readModel.Nodes.First(n => n.NodeId == RowsLandmarkViewModel.SelectedRow.NodeId);
            var errorMessage = OneLandmarkViewModel.GpsInputSmallViewModel.TryGetPoint(out PointLatLng position);
            if (errorMessage != null)
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, errorMessage);
                _windowManager.ShowDialogWithAssignedOwner(vm);
                return;
            }
            node.Position = position;
            node.IsHighlighted = true;
            _graphReadModel.MainMap.SetPositionWithoutFiringEvent(node.Position);
            await _graphReadModel.RefreshVisiblePart();

            var nodeVm = _graphReadModel.Data.Nodes.First(n => n.Id == RowsLandmarkViewModel.SelectedRow.NodeId);
            nodeVm.Position = position;
            nodeVm.IsHighlighted = true;
        }

        public async void RestoreCoordinates()
        {
            // restore
            OneLandmarkViewModel.RestoreCoordinates();
            //
            await ShowLandmark();
        }
        #endregion

        public override void CanClose(Action<bool> callback)
        {
            if (RowsLandmarkViewModel.Command.Any())
            {
                var vm = new MyMessageBoxViewModel(MessageType.Confirmation,
                    Resources.SID_All_changes_will_be_canceled__Continue_);
                _windowManager.ShowDialogWithAssignedOwner(vm);
                if (!vm.IsAnswerPositive) return;
                RowsLandmarkViewModel.CancelAllChanges();
            }
            base.CanClose(callback);
        }
    }
}
