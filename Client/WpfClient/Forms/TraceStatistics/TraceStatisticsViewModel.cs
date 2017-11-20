using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class TraceStatisticsViewModel : Screen
    {
        private readonly ReadModel _readModel;
        private readonly IWcfServiceForClient _c2DWcfManager;

        public string RtuTitle { get; set; }
        public string PortNumber { get; set; }
        public List<BaseRefForStats> BaseRefs { get; set; }
        public List<MeasurementVm> Rows { get; set; }

        public TraceStatisticsViewModel(ReadModel readModel, IWcfServiceForClient c2DWcfManager)
        {
            _readModel = readModel;
            _c2DWcfManager = c2DWcfManager;
        }

        public bool Initialize(Guid traceId)
        {
            var trace = _readModel.Traces.FirstOrDefault(t => t.Id == traceId);
            if (trace == null)
                return false;
            RtuTitle = _readModel.Rtus.FirstOrDefault(r => r.Id == trace.RtuId)?.Title;
            PortNumber = trace.OtauPort.IsPortOnMainCharon
                ? trace.OtauPort.OpticalPort.ToString()
                : $@"{trace.OtauPort.OtauIp}:{trace.OtauPort.OtauTcpPort}-{trace.OtauPort.OpticalPort}";

            var traceStatistics = _c2DWcfManager.GetTraceStatistics(traceId).Result;
            if (traceStatistics == null)
                return false;

            BaseRefs = traceStatistics.BaseRefs;

            Rows = new List<MeasurementVm>();
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
    }
}
