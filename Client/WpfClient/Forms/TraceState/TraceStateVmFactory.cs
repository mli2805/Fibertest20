using System;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.Client
{
    public class TraceStateVmFactory
    {
        private readonly IMyLog _logFile;
        private readonly ReadModel _readModel;
        private readonly C2DWcfManager _c2DWcfManager;

        public TraceStateVmFactory(IMyLog logFile, ReadModel readModel, C2DWcfManager c2DWcfManager)
        {
            _logFile = logFile;
            _readModel = readModel;
            _c2DWcfManager = c2DWcfManager;
        }

        // TraceLeaf
        public async Task<TraceStateVm> Create(Guid traceId)
        {
            Measurement measurement = await GetLastTraceMeasurement(traceId);
            return CreateVm(measurement);
        }

        // Trace statistics
        // Server
        public TraceStateVm CreateVm(Measurement measurement)
        {
            var vm = new TraceStateVm();
            PrepareCaption(measurement.TraceId, ref vm);

            vm.TraceId = measurement.TraceId;
            vm.TraceState = measurement.TraceState;
            vm.BaseRefType = measurement.BaseRefType;
            vm.MeasurementTimestamp = measurement.MeasurementTimestamp;
            vm.SorFileId = measurement.SorFileId;

            vm.EventStatus = measurement.EventStatus;
            vm.OpticalEventComment = measurement.Comment;

            return vm;
        }

        public TraceStateVm CreateVm(OpticalEventVm opticalEventVm)
        {
            var vm = new TraceStateVm();
            PrepareCaption(opticalEventVm.TraceId, ref vm);

            vm.TraceId = opticalEventVm.TraceId;
            vm.TraceState = opticalEventVm.TraceState;
            vm.BaseRefType = opticalEventVm.BaseRefType;
            vm.MeasurementTimestamp = opticalEventVm.MeasurementTimestamp;
            vm.SorFileId = opticalEventVm.SorFileId;

            vm.OpticalEventId = opticalEventVm.Nomer;
            vm.EventStatus = opticalEventVm.EventStatus;
            vm.OpticalEventComment = opticalEventVm.Comment;

           return vm;

        }

        private void PrepareCaption(Guid traceId, ref TraceStateVm result)
        {
            var trace = _readModel.Traces.FirstOrDefault(t => t.Id == traceId);
            if (trace == null)
                return;

            result.TraceTitle = trace.Title;
            result.RtuTitle = _readModel.Rtus.FirstOrDefault(r => r.Id == trace.RtuId)?.Title;
            result.PortTitle = trace.OtauPort == null ? Resources.SID__not_attached_ : trace.OtauPort.IsPortOnMainCharon
                ? trace.OtauPort.OpticalPort.ToString()
                : $@"{trace.OtauPort.OtauIp}:{trace.OtauPort.OtauTcpPort}-{trace.OtauPort.OpticalPort}";
        }

        private  async Task<Measurement> GetLastTraceMeasurement(Guid traceId)
        {
            var measurement = await _c2DWcfManager.GetLastMeasurementForTrace(traceId);
            if (measurement == null)
            {
                _logFile.AppendLine($@"Cannot get last measurement for trace {traceId.First6()}");
                return null;
            }
            else
            {
                _logFile.AppendLine($@"Last measurement for trace {traceId.First6()} recieved");
                return measurement;
            }
        }
    }
}