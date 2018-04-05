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
        public List<GpsInputModeComboItem> GpsInputModes { get; set; } =
            (from mode in Enum.GetValues(typeof(GpsInputMode)).OfType<GpsInputMode>()
             select new GpsInputModeComboItem(mode)).ToList();

        private GpsInputModeComboItem _selectedGpsInputMode;
        public GpsInputModeComboItem SelectedGpsInputMode
        {
            get { return _selectedGpsInputMode; }
            set
            {
                if (Equals(value, _selectedGpsInputMode)) return;
                _selectedGpsInputMode = value;
                Rows = LandmarksToRows();
            }
        }

        public List<Trace> Traces { get; set; }

        private Trace _selectedTrace;
        public Trace SelectedTrace
        {
            get { return _selectedTrace; }
            set
            {
                if (Equals(value, _selectedTrace)) return;
                _selectedTrace = value;

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
            get { return _isFilterOn; }
            set
            {
                if (value == _isFilterOn) return;
                _isFilterOn = value;
                Rows = LandmarksToRows();
            }
        }

        private readonly ILifetimeScope _globalScope;
        private readonly Model _readModel;
        private readonly IWindowManager _windowManager;
        private readonly IWcfServiceForClient _c2DWcfManager;
        private List<Landmark> _landmarks;

        private ObservableCollection<LandmarkRow> _rows;
        public ObservableCollection<LandmarkRow> Rows
        {
            get { return _rows; }
            set
            {
                if (Equals(value, _rows)) return;
                _rows = value;
                NotifyOfPropertyChange();
            }
        }

        public LandmarkRow SelectedRow { get; set; }

        public LandmarksViewModel(ILifetimeScope globalScope, Model readModel,
            IWindowManager windowManager, IWcfServiceForClient c2DWcfManager)
        {
            _globalScope = globalScope;
            _readModel = readModel;
            _windowManager = windowManager;
            _c2DWcfManager = c2DWcfManager;
            _selectedGpsInputMode = GpsInputModes.Last();
        }

        public async Task<int> Initialize(Guid id, bool isUserClickedOnRtu)
        {
            if (isUserClickedOnRtu)
            {
                Traces = _readModel.Traces.Where(t => t.RtuId == id).ToList();
                _selectedTrace = Traces.First();
            }
            else
            {
                var trace = _readModel.Traces.First(t => t.TraceId == id);
                Traces = _readModel.Traces.Where(t => t.RtuId == trace.RtuId).ToList();
                _selectedTrace = _readModel.Traces.First(t => t.TraceId == id);
            }
            return await PrepareLandmarks();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = string.Format(Resources.SID_Landmarks_of_trace__0_, SelectedTrace.Title);
        }

        private async Task<int> PrepareLandmarks()
        {
            if (SelectedTrace.PreciseId == Guid.Empty)
                _landmarks = new LandmarksGraphParser(_readModel).GetLandmarks(SelectedTrace);
            else
            {
                var sorData = await GetBase(SelectedTrace.PreciseId);
                _landmarks = new LandmarksBaseParser().GetLandmarks(sorData);
            }
            Rows = LandmarksToRows();
            return 0;
        }

        private async Task<OtdrDataKnownBlocks> GetBase(Guid baseId)
        {
            if (baseId == Guid.Empty)
                return null;

            var baseRef = _readModel.BaseRefs.First(b => b.Id == baseId);
            var sorBytes = await _c2DWcfManager.GetSorBytes(baseRef.SorFileId);
            return SorData.FromBytes(sorBytes);
        }

        public async void ChangeTrace()
        {
            await PrepareLandmarks();
            DisplayName = string.Format(Resources.SID_Landmarks_of_trace__0_, SelectedTrace.Title);
        }

        public void ShowReflectogram() { }

        public void ShowInformation()
        {
            var vm = _globalScope.Resolve<LandmarkViewModel>();
            var landmark = _landmarks.First(l => l.Number == SelectedRow.Number);
            vm.Initialize(landmark);
            vm.RtuTitle = _readModel.Rtus.First(r => r.Id == _selectedTrace.RtuId).Title;
            vm.TraceTitle = _selectedTrace.Title;
            _windowManager.ShowWindowWithAssignedOwner(vm);
        }
    }
}
