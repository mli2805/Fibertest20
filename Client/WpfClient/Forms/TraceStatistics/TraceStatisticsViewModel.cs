using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class TraceStatisticsViewModel : Screen
    {
        private readonly IMyLog _logFile;
        private readonly ReadModel _readModel;
        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly ReflectogramManager _reflectogramManager;
        private readonly TraceStateViewsManager _traceStateViewsManager;
        public bool IsOpen { get; private set; }

        public string TraceTitle { get; set; }
        public string RtuTitle { get; set; }
        public string PortNumber { get; set; }

        private BaseRefForStats _selectedBaseRef;
        public BaseRefForStats SelectedBaseRef
        {
            get { return _selectedBaseRef; }
            set
            {
                if (Equals(value, _selectedBaseRef)) return;
                _selectedBaseRef = value;
                NotifyOfPropertyChange();
            }
        }

        public ObservableCollection<BaseRefForStats> BaseRefs { get; set; } = new ObservableCollection<BaseRefForStats>();

        public ObservableCollection<MeasurementModel> Rows { get; set; } = new ObservableCollection<MeasurementModel>();

        private MeasurementModel _selectedRow;
        public MeasurementModel SelectedRow
        {
            get { return _selectedRow; }
            set
            {
                if (Equals(value, _selectedRow)) return;
                _selectedRow = value;
                NotifyOfPropertyChange();
            }
        }

        public TraceStatisticsViewModel(IMyLog logFile, ReadModel readModel, IWcfServiceForClient c2DWcfManager,
            ReflectogramManager reflectogramManager, TraceStateViewsManager traceStateViewsManager)
        {
            _logFile = logFile;
            _readModel = readModel;
            _c2DWcfManager = c2DWcfManager;
            _reflectogramManager = reflectogramManager;
            _traceStateViewsManager = traceStateViewsManager;

            var view = CollectionViewSource.GetDefaultView(Rows);
            view.SortDescriptions.Add(new SortDescription(@"Measurement.SorFileId", ListSortDirection.Descending));
        }

        public async Task Initialize(Guid traceId)
        {
            var trace = _readModel.Traces.FirstOrDefault(t => t.Id == traceId);
            if (trace == null)
                return;
            TraceTitle = trace.Title;
            RtuTitle = _readModel.Rtus.FirstOrDefault(r => r.Id == trace.RtuId)?.Title;
            PortNumber = trace.OtauPort == null ? Resources.SID__not_attached_ : trace.OtauPort.IsPortOnMainCharon
                ? trace.OtauPort.OpticalPort.ToString()
                : $@"{trace.OtauPort.OtauIp}:{trace.OtauPort.OtauTcpPort}-{trace.OtauPort.OpticalPort}";

            var traceStatistics = await _c2DWcfManager.GetTraceStatistics(traceId);
            if (traceStatistics == null)
                return;
            _logFile.AppendLine($@"There {traceStatistics.BaseRefs.Count} base refs and {traceStatistics.Measurements.Count} measurements");

            BaseRefs.Clear();
            foreach (var baseRef in traceStatistics.BaseRefs)
            {
                BaseRefs.Add(baseRef);   
            }

            Rows.Clear();
            foreach (var measurement in traceStatistics.Measurements)
                Rows.Add(new MeasurementModel(measurement));
        }

        public void AddNewMeasurement(Measurement measurement)
        {
            Rows.Add(new MeasurementModel(measurement));
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = @"Trace statistics";
            IsOpen = true;
        }

        public void ShowReflectogram(int param)
        {
            _reflectogramManager.SetTempFileName(TraceTitle, SelectedRow.Measurement.SorFileId, SelectedRow.Measurement.EventRegistrationTimestamp);
            if (param == 2)
                _reflectogramManager.ShowRefWithBase(SelectedRow.Measurement.SorFileId);
            else
                _reflectogramManager.ShowOnlyCurrentMeasurement(SelectedRow.Measurement.SorFileId);
        }

        public void SaveReflectogramAs(bool param)
        {
            _reflectogramManager.SetTempFileName(TraceTitle, SelectedRow.Measurement.SorFileId, SelectedRow.Measurement.MeasurementTimestamp);
            _reflectogramManager.SaveReflectogramAs(SelectedRow.Measurement.SorFileId, param);
        }

        public void ShowBaseReflectogram()
        {
            _reflectogramManager.SetTempFileName(TraceTitle, SelectedBaseRef.BaseRefTypeString, SelectedBaseRef.AssignedAt);
            _reflectogramManager.ShowBaseReflectogram(SelectedBaseRef.BaseRefId);
        }

        public void SaveBaseReflectogramAs()
        {
            _reflectogramManager.SetTempFileName(TraceTitle, SelectedBaseRef.BaseRefTypeString, SelectedBaseRef.AssignedAt);
            _reflectogramManager.SaveBaseReflectogramAs(SelectedBaseRef.BaseRefId);
        }

        public void ShowRftsEvents()
        {
            _reflectogramManager.ShowRftsEvents(SelectedRow.Measurement.SorFileId);
        }

        public void ShowTraceState()
        {                 
            var lastRow = Rows.First(); // click on the Row , so Rows collection couldn't be empty
            _traceStateViewsManager.ShowTraceState(SelectedRow.Measurement, lastRow.Measurement.Id == SelectedRow.Measurement.Id);
        }

        public override void CanClose(Action<bool> callback)
        {
            IsOpen = false;
            base.CanClose(callback);
        }
    }
}
