using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Caliburn.Micro;
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
            RowsLandmarkViewModel rowsLandmarkViewModel)
        {
            CurrentGis = currentGis;
            _globalScope = globalScope;
            _readModel = readModel;
            _windowManager = windowManager;
            _selectedGpsInputMode = GpsInputModes.First(i => i.Mode == CurrentGis.GpsInputMode);
            GisVisibility = currentGis.IsGisOn ? Visibility.Visible : Visibility.Collapsed;

            RowsLandmarkViewModel = rowsLandmarkViewModel;
            RowsLandmarkViewModel.PropertyChanged += RowsLandmarkViewModel_PropertyChanged;
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
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Landmarks_of_traces_of_RTU_ + _rtu.Title;
        }

        public async Task RefreshOnChangedTrace()
        {
            await RowsLandmarkViewModel.Initialize(SelectedTrace, Guid.Empty, 0);
        }

        private void RowsLandmarkViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == @"AreThereAnyChanges")
            {
                AreThereAnyChanges = RowsLandmarkViewModel.AreThereAnyChanges;
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
            AreThereAnyChanges = false;

            await RowsLandmarkViewModel
                .Initialize(_selectedTrace, Guid.Empty, RowsLandmarkViewModel.SelectedRow.Number);
        }

        public void CancelAllChanges()
        {
            RowsLandmarkViewModel.CancelAllChanges();
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
