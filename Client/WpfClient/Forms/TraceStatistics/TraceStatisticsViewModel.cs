using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class TraceStatisticsViewModel : Screen
    {
        private readonly ReadModel _readModel;
        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly ReflectogramManager _reflectogramManager;
        private readonly TraceStateViewsManager _traceStateViewsManager;

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

        public List<BaseRefForStats> BaseRefs { get; set; }

        public ObservableCollection<MeasurementVm> Rows { get; set; } = new ObservableCollection<MeasurementVm>();

        private MeasurementVm _selectedRow;
        public MeasurementVm SelectedRow
        {
            get { return _selectedRow; }
            set
            {
                if (Equals(value, _selectedRow)) return;
                _selectedRow = value;
                NotifyOfPropertyChange();
            }
        }

        public TraceStatisticsViewModel(ReadModel readModel, IWcfServiceForClient c2DWcfManager,
            ReflectogramManager reflectogramManager, TraceStateViewsManager traceStateViewsManager)
        {
            _readModel = readModel;
            _c2DWcfManager = c2DWcfManager;
            _reflectogramManager = reflectogramManager;
            _traceStateViewsManager = traceStateViewsManager;

            var view = CollectionViewSource.GetDefaultView(Rows);
            view.SortDescriptions.Add(new SortDescription(@"Measurement.SorFileId", ListSortDirection.Descending));
        }

        public async void Initialize(Guid traceId)
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

            BaseRefs = traceStatistics.BaseRefs;

            Rows.Clear();
            foreach (var measurement in traceStatistics.Measurements)
                Rows.Add(new MeasurementVm(measurement));
        }

        public void AddNewMeasurement(Measurement measurement)
        {
            Rows.Add(new MeasurementVm(measurement));
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = @"Trace statistics";
        }

        public void ShowReflectogram(int param)
        {
            if (param == 2)
                _reflectogramManager.ShowRefWithBase(SelectedRow.Measurement.SorFileId);
            else
                _reflectogramManager.ShowOnlyCurrentMeasurement(SelectedRow.Measurement.SorFileId);
        }

        public void SaveReflectogramAs(bool param)
        {
            var timestamp = $@"{SelectedRow.Measurement.MeasurementTimestamp:dd-MM-yyyy HH-mm-ss}";
            var defaultFilename = $@"{TraceTitle} [N{SelectedRow.Measurement.SorFileId}] {timestamp}";
            _reflectogramManager.SaveReflectogramAs(SelectedRow.Measurement.SorFileId, defaultFilename, param);
        }

        public void ShowBaseReflectogram()
        {
            _reflectogramManager.ShowBaseReflectogram(SelectedBaseRef.BaseRefId);
        }

        public void SaveBaseReflectogramAs()
        {
            var partFilename = $@"{TraceTitle} [{SelectedBaseRef.BaseRefType.GetLocalizedString()}] ";
            _reflectogramManager.SaveBaseReflectogramAs(SelectedBaseRef.BaseRefId, partFilename);
        }

        public void ShowRftsEvents()
        {
            _reflectogramManager.ShowRftsEvents(SelectedRow.Measurement.SorFileId);
        }

        public void ShowTraceState()
        {                 
            var lastRow = Rows.First(); // click on the Row , so Row couldn't be null
            _traceStateViewsManager.ShowTraceState(SelectedRow.Measurement, lastRow.Measurement.Id == SelectedRow.Measurement.Id);
        }
     
    }
}
