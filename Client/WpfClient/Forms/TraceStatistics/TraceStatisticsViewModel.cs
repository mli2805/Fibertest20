using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.WcfServiceForClientInterface;
using Trace = Iit.Fibertest.Graph.Trace;

namespace Iit.Fibertest.Client
{
    public class TraceStatisticsViewModel : Screen
    {
        private readonly ReadModel _readModel;
        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly ReflectogramManager _reflectogramManager;
        private readonly TraceStateManager _traceStateManager;
        private Trace _trace;

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
            ReflectogramManager reflectogramManager, TraceStateManager traceStateManager)
        {
            _readModel = readModel;
            _c2DWcfManager = c2DWcfManager;
            _reflectogramManager = reflectogramManager;
            _traceStateManager = traceStateManager;

            var view = CollectionViewSource.GetDefaultView(Rows);
            view.SortDescriptions.Add(new SortDescription(@"SorFileId", ListSortDirection.Descending));
        }

        public bool Initialize(Guid traceId)
        {
            _trace = _readModel.Traces.FirstOrDefault(t => t.Id == traceId);
            if (_trace == null)
                return false;
            TraceTitle = _trace.Title;
            RtuTitle = _readModel.Rtus.FirstOrDefault(r => r.Id == _trace.RtuId)?.Title;
            PortNumber = _trace.OtauPort.IsPortOnMainCharon
                ? _trace.OtauPort.OpticalPort.ToString()
                : $@"{_trace.OtauPort.OtauIp}:{_trace.OtauPort.OtauTcpPort}-{_trace.OtauPort.OpticalPort}";

            var traceStatistics = _c2DWcfManager.GetTraceStatistics(traceId).Result;
            if (traceStatistics == null)
                return false;

            BaseRefs = traceStatistics.BaseRefs;

            Rows.Clear();
            foreach (var measurement in traceStatistics.Measurements)
            {
                Rows.Add(new MeasurementVm()
                {
                    BaseRefType = measurement.BaseRefType,
                    TraceState = measurement.TraceState,
                    Timestamp = measurement.Timestamp,
                    SorFileId = measurement.SorFileId,
                    IsOpticalEvent = measurement.IsOpticalEvent,
                });
            }

            return true;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = @"Trace statistics";
        }

        public void ShowReflectogram(int param)
        {
            if (param == 2)
                _reflectogramManager.ShowRefWithBase(SelectedRow.SorFileId);
            else
                _reflectogramManager.ShowOnlyCurrentMeasurement(SelectedRow.SorFileId);
        }

        public void SaveReflectogramAs(bool param)
        {
            var timestamp = $@"{SelectedRow.Timestamp:dd-MM-yyyy HH-mm-ss}";
            var defaultFilename = $@"{TraceTitle} [N{SelectedRow.SorFileId}] {timestamp}";
            _reflectogramManager.SaveReflectogramAs(SelectedRow.SorFileId, defaultFilename, param);
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
            _reflectogramManager.ShowRftsEvents(SelectedRow.SorFileId);
        }

        public void ShowTraceState()
        {
           _traceStateManager.ShowTraceState(_trace.Id, SelectedRow);
        }
     
    }
}
