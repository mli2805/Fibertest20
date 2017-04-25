using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.IitOtdrLibrary;
using Iit.Fibertest.StringResources;
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
                _landmarks = (SelectedTrace.PreciseId == Guid.Empty) ?
                    new LandmarksGraphParser(_readModel).GetLandmarks(SelectedTrace) :
                    new LandmarksBaseParser().GetLandmarks(GetBase(SelectedTrace.PreciseId));
                Rows = LandmarksToRows();
                DisplayName = string.Format(Resources.SID_Landmarks_of_trace__0_, SelectedTrace.Title);
            }
        }

        private ObservableCollection<LandmarkRow> LandmarksToRows()
        {
            var temp = _isFilterOn ? _landmarks.Where(l => l.EquipmentType != EquipmentType.Well) : _landmarks;
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

        private readonly ReadModel _readModel;
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

        public LandmarksViewModel(ReadModel readModel)
        {
            _readModel = readModel;
            _selectedGpsInputMode = GpsInputModes.Last();
        }

        public void Initialize(Guid id, bool isUserClickedOnRtu = false)
        {
            if (isUserClickedOnRtu)
            {
                Traces = _readModel.Traces.Where(t => t.RtuId == id).ToList();
                SelectedTrace = Traces.First();
            }
            else
            {
                SelectedTrace = _readModel.Traces.First(t => t.Id == id);
                Traces = _readModel.Traces.Where(t => t.RtuId == SelectedTrace.RtuId).ToList();
            }
        }

        private OtdrDataKnownBlocks GetBase(Guid baseId)
        {
            var bytes = File.ReadAllBytes(@"c:\temp\base.sor");
            // TODO get sordata from database
            return SorData.FromBytes(bytes);
        }
    }
}
