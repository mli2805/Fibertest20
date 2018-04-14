using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.IitOtdrLibrary;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfServiceForClientInterface;
using Optixsoft.SorExaminer.OtdrDataFormat;

namespace Iit.Fibertest.Client
{
    public class LandmarksViewModel : Screen
    {
        public CurrentGpsInputMode CurrentGpsInputMode { get; }

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
                CurrentGpsInputMode.Mode = _selectedGpsInputMode.Mode;
                RefreshCoorsInRows();
            }
        }

        private void RefreshCoorsInRows()
        {
            foreach (var row in Rows)
            {
                var landmark = _landmarks.First(l => l.NodeId == row.NodeId);
                row.GpsCoors = landmark.GpsCoors.ToDetailedString(_selectedGpsInputMode.Mode);
            }
        }

        private ObservableCollection<LandmarkRow> LandmarksToRows()
        {
            var temp = _isFilterOn ? _landmarks.Where(l => l.EquipmentType != EquipmentType.EmptyNode) : _landmarks;
            return new ObservableCollection<LandmarkRow>(temp.Select(l => l.ToRow(_selectedGpsInputMode.Mode)));
        }

        private bool _isFilterOn;
        public bool IsFilterOn
        {
            get => _isFilterOn;
            set
            {
                if (value == _isFilterOn) return;
                _isFilterOn = value;
                Rows = LandmarksToRows();
            }
        }

        private readonly ILifetimeScope _globalScope;
        private readonly Model _readModel;
        private readonly LandmarksBaseParser _landmarksBaseParser;
        private readonly LandmarksGraphParser _landmarksGraphParser;
        private readonly IWcfServiceForClient _c2DWcfManager;
        private List<Landmark> _landmarks;

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
                OneLandmarkViewModel.Cancel();
                OneLandmarkViewModel.SelectedLandmark = (Landmark)_landmarks.First(l => l.NodeId == SelectedRow.NodeId).Clone();
                NotifyOfPropertyChange();
            }
        }

        public OneLandmarkViewModel OneLandmarkViewModel { get; set; }

        public LandmarksViewModel(ILifetimeScope globalScope, Model readModel, CurrentGpsInputMode currentGpsInputMode,
            LandmarksBaseParser landmarksBaseParser, LandmarksGraphParser landmarksGraphParser,
             IWcfServiceForClient c2DWcfManager)
        {
            CurrentGpsInputMode = currentGpsInputMode;
            _globalScope = globalScope;
            _readModel = readModel;
            _landmarksBaseParser = landmarksBaseParser;
            _landmarksGraphParser = landmarksGraphParser;
            _c2DWcfManager = c2DWcfManager;
            _selectedGpsInputMode = GpsInputModes.First(i => i.Mode == CurrentGpsInputMode.Mode);
        }

        private async Task<int> Initialize()
        {
            OneLandmarkViewModel = _globalScope.Resolve<OneLandmarkViewModel>();
            var res = await PrepareLandmarks();
            SelectedRow = Rows.First();
            return res;
        }

        public async Task<int> InitializeFromRtu(Guid rtuId)
        {
            Traces = _readModel.Traces.Where(t => t.RtuId == rtuId).ToList();
            if (Traces.Count == 0) return -1;
            _selectedTrace = Traces.First();

            return await Initialize();
        }

        public async Task<int> InitializeFromTrace(Guid traceId)
        {
            var trace = _readModel.Traces.First(t => t.TraceId == traceId);
            Traces = _readModel.Traces.Where(t => t.RtuId == trace.RtuId).ToList();
            _selectedTrace = _readModel.Traces.First(t => t.TraceId == traceId);

            return await Initialize();
        }

        public async Task<int> InitializeFromNode(Guid nodeId)
        {
            var trace = _readModel.Traces.FirstOrDefault(t => t.NodeIds.Contains(nodeId));
            if (trace == null) return -1;
            Traces = _readModel.Traces.Where(t => t.RtuId == trace.RtuId).ToList();
            _selectedTrace = Traces.First(t => t.NodeIds.Contains(nodeId));

            var res = await Initialize();
            SelectedRow = Rows.First(r => r.NodeId == nodeId);
            return res;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = string.Format(Resources.SID_Landmarks_of_trace__0_, SelectedTrace.Title);
        }

        private async Task<int> PrepareLandmarks()
        {
            OneLandmarkViewModel.TraceTitle = SelectedTrace.Title;
            if (SelectedTrace.PreciseId == Guid.Empty)
                _landmarks = _landmarksGraphParser.GetLandmarks(SelectedTrace);
            else
            {
                var sorData = await GetBase(SelectedTrace.PreciseId);
                _landmarks = _landmarksBaseParser.GetLandmarks(sorData, SelectedTrace);
            }
            Rows = LandmarksToRows();
            return 0;
        }

        private async Task<OtdrDataKnownBlocks> GetBase(Guid baseId)
        {
            if (baseId == Guid.Empty)
                return null;

            var baseRef = _readModel.BaseRefs.First(b => b.Id == baseId);
            OneLandmarkViewModel.SorFileId = baseRef.SorFileId;
            OneLandmarkViewModel.PreciseTimestamp = baseRef.SaveTimestamp;
            var sorBytes = await _c2DWcfManager.GetSorBytes(baseRef.SorFileId);
            return SorData.FromBytes(sorBytes);
        }

        public async Task<int> RefreshOrChangeTrace() // button
        {
            await PrepareLandmarks();
            OneLandmarkViewModel = _globalScope.Resolve<OneLandmarkViewModel>();
            SelectedRow = Rows.First();
            DisplayName = string.Format(Resources.SID_Landmarks_of_trace__0_, SelectedTrace.Title);
            return 0;
        }

        public async void RefreshAsChangesReaction()
        {
            var index = Rows.IndexOf(SelectedRow);

            await PrepareLandmarks();
            OneLandmarkViewModel = _globalScope.Resolve<OneLandmarkViewModel>();
            SelectedRow = Rows[index];
        }

        public override void CanClose(Action<bool> callback)
        {
            OneLandmarkViewModel.Cancel();
            base.CanClose(callback);
        }
    }
}
