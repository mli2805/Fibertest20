﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class TraceStatisticsViewModel : Screen
    {
        private readonly Model _readModel;
        private readonly ReflectogramManager _reflectogramManager;
        private readonly TraceStateViewsManager _traceStateViewsManager;
        private readonly BaseRefModelFactory _baseRefModelFactory;

        private Trace _trace;
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

        public TraceStatisticsViewModel(Model readModel, ReflectogramManager reflectogramManager,
            TraceStateViewsManager traceStateViewsManager, BaseRefModelFactory baseRefModelFactory)
        {
            _readModel = readModel;
            _reflectogramManager = reflectogramManager;
            _traceStateViewsManager = traceStateViewsManager;
            _baseRefModelFactory = baseRefModelFactory;

            var view = CollectionViewSource.GetDefaultView(Rows);
            view.SortDescriptions.Add(new SortDescription(@"Measurement.SorFileId", ListSortDirection.Descending));
        }

        public void Initialize(Guid traceId)
        {
            _trace = _readModel.Traces.FirstOrDefault(t => t.TraceId == traceId);
            if (_trace == null)
                return;
            TraceTitle = _trace.Title;
            RtuTitle = _readModel.Rtus.FirstOrDefault(r => r.Id == _trace.RtuId)?.Title;
            PortNumber = _trace.OtauPort == null ? Resources.SID__not_attached_ : _trace.OtauPort.IsPortOnMainCharon
                ? _trace.OtauPort.OpticalPort.ToString()
                //: $@"{_trace.OtauPort.OtauIp}:{_trace.OtauPort.OtauTcpPort}-{_trace.OtauPort.OpticalPort}";
                : $@"{_trace.OtauPort.Serial}-{_trace.OtauPort.OpticalPort}";

            BaseRefs.Clear();
            foreach (var baseRef in _readModel.BaseRefs.Where(b => b.TraceId == traceId))
            {
                BaseRefs.Add(_baseRefModelFactory.Create(baseRef));
            }

            Rows.Clear();
            foreach (var measurement in _readModel.Measurements.Where(m => m.TraceId == traceId).OrderBy(t => t.MeasurementTimestamp))
                Rows.Add(new MeasurementModel(measurement));
        }

        public void AddNewMeasurement()
        {
            var lastMeasurement = _readModel.Measurements.Last(m => m.TraceId == _trace.TraceId);
            Rows.Add(new MeasurementModel(lastMeasurement));
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Trace_statistics;
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
            var lastRow = Rows.First(); // click on the Row , so Rows collection couldn't be empty
            _traceStateViewsManager.ShowTraceState(SelectedRow.Measurement, lastRow.Measurement.SorFileId == SelectedRow.Measurement.SorFileId);
        }

        public override void CanClose(Action<bool> callback)
        {
            IsOpen = false;
            base.CanClose(callback);
        }

        public void Close()
        {
            TryClose();
        }
    }
}
