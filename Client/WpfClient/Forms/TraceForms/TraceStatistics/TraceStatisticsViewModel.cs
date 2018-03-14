using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class TraceStatistics
    {
        public List<Measurement> Measurements { get; set; }

        public List<BaseRefModel> BaseRefs { get; set; }
    }

    public class TraceStatisticsViewModel : Screen
    {
        private readonly ReadModel _readModel;
        private readonly ReflectogramManager _reflectogramManager;
        private readonly BaseRefModelFactory _baseRefModelFactory;
        public bool IsOpen { get; private set; }

        public string TraceTitle { get; set; }
        public string RtuTitle { get; set; }
        public string PortNumber { get; set; }

        private BaseRefModel _selectedBaseRef;
        public BaseRefModel SelectedBaseRef
        {
            get { return _selectedBaseRef; }
            set
            {
                if (Equals(value, _selectedBaseRef)) return;
                _selectedBaseRef = value;
                NotifyOfPropertyChange();
            }
        }

        public ObservableCollection<BaseRefModel> BaseRefs { get; set; } = new ObservableCollection<BaseRefModel>();

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

        public TraceStatisticsViewModel(ReadModel readModel, ReflectogramManager reflectogramManager, BaseRefModelFactory baseRefModelFactory)
        {
            _readModel = readModel;
            _reflectogramManager = reflectogramManager;
            _baseRefModelFactory = baseRefModelFactory;

            var view = CollectionViewSource.GetDefaultView(Rows);
            view.SortDescriptions.Add(new SortDescription(@"Measurement.SorFileId", ListSortDirection.Descending));
        }

        public void Initialize(Guid traceId)
        {
            var trace = _readModel.Traces.FirstOrDefault(t => t.Id == traceId);
            if (trace == null)
                return;
            TraceTitle = trace.Title;
            RtuTitle = _readModel.Rtus.FirstOrDefault(r => r.Id == trace.RtuId)?.Title;
            PortNumber = trace.OtauPort == null ? Resources.SID__not_attached_ : trace.OtauPort.IsPortOnMainCharon
                ? trace.OtauPort.OpticalPort.ToString()
                : $@"{trace.OtauPort.OtauIp}:{trace.OtauPort.OtauTcpPort}-{trace.OtauPort.OpticalPort}";

            BaseRefs.Clear();
            foreach (var baseRef in _readModel.BaseRefs.Where(b => b.TraceId == traceId))
            {
                BaseRefs.Add(_baseRefModelFactory.Create(baseRef));   
            }

            Rows.Clear();
            foreach (var measurement in _readModel.Measurements.Where(m=>m.TraceId == traceId).OrderBy(t=>t.MeasurementTimestamp))
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
            // do not use localized base ref type!
            _reflectogramManager.SetTempFileName(TraceTitle, SelectedBaseRef.BaseRefType.ToString(), SelectedBaseRef.AssignedAt);
            _reflectogramManager.ShowBaseReflectogram(SelectedBaseRef.SorFileId);
        }

        public void SaveBaseReflectogramAs()
        {
            // do not use localized base ref type!
            _reflectogramManager.SetTempFileName(TraceTitle, SelectedBaseRef.BaseRefType.ToString(), SelectedBaseRef.AssignedAt);
            _reflectogramManager.SaveBaseReflectogramAs(SelectedBaseRef.SorFileId);
        }

        public void ShowRftsEvents()
        {
            _reflectogramManager.ShowRftsEvents(SelectedRow.Measurement.SorFileId);
        }

        public void ShowTraceState()
        {                 
     //       var lastRow = Rows.First(); // click on the Row , so Rows collection couldn't be empty
     //       _traceStateViewsManager.ShowTraceState(SelectedRow.Measurement, lastRow.Measurement.Id == SelectedRow.Measurement.Id);
        }

        public override void CanClose(Action<bool> callback)
        {
            IsOpen = false;
            base.CanClose(callback);
        }
    }
}
