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
using Trace = Iit.Fibertest.Graph.Trace;

namespace Iit.Fibertest.Client
{
    public class TraceStatisticsViewModel : Screen
    {
        private readonly ReadModel _readModel;
        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly MeasurementManager _measurementManager;
        private Trace _trace;

        public string RtuTitle { get; set; }
        public string PortNumber { get; set; }
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

        public TraceStatisticsViewModel(ReadModel readModel, IWcfServiceForClient c2DWcfManager, MeasurementManager measurementManager)
        {
            _readModel = readModel;
            _c2DWcfManager = c2DWcfManager;
            _measurementManager = measurementManager;

            var view = CollectionViewSource.GetDefaultView(Rows);
            view.SortDescriptions.Add(new SortDescription(@"Nomer",ListSortDirection.Descending));
        }

        public bool Initialize(Guid traceId)
        {
            _trace = _readModel.Traces.FirstOrDefault(t => t.Id == traceId);
            if (_trace == null)
                return false;
            RtuTitle = _readModel.Rtus.FirstOrDefault(r => r.Id == _trace.RtuId)?.Title;
            PortNumber = _trace.OtauPort.IsPortOnMainCharon
                ? _trace.OtauPort.OpticalPort.ToString()
                : $@"{_trace.OtauPort.OtauIp}:{_trace.OtauPort.OtauTcpPort}-{_trace.OtauPort.OpticalPort}";

            var traceStatistics = _c2DWcfManager.GetTraceStatistics(traceId).Result;
            if (traceStatistics == null)
                return false;

            BaseRefs = traceStatistics.BaseRefs;

            foreach (var measurement in traceStatistics.Measurements)
            {
                Rows.Add(new MeasurementVm()
                {
                    Nomer = measurement.Id,
                    BaseRefType = measurement.BaseRefType,
                    TraceState = measurement.TraceState,
                    Timestamp = measurement.Timestamp,
                    MeasurementId = measurement.MeasurementId,
                });
            }

            return true;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = @"Trace statistics";
        }

        public void ShowReflectogram()
        {
            _measurementManager.ShowReflectogram(SelectedRow.MeasurementId);
        }

        public void ShowRftsEvents()
        {
            _measurementManager.ShowRftsEvents(SelectedRow.MeasurementId);
        }
    }
}
