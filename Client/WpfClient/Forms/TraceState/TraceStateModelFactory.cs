using System;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.Client
{
    public class TraceStateModelFactory
    {
        private readonly IMyLog _logFile;
        private readonly ReadModel _readModel;
        private readonly C2DWcfManager _c2DWcfManager;

        public TraceStateModelFactory(IMyLog logFile, ReadModel readModel, C2DWcfManager c2DWcfManager)
        {
            _logFile = logFile;
            _readModel = readModel;
            _c2DWcfManager = c2DWcfManager;
        }

        // TraceLeaf
        public async Task<TraceStateModel> Create(Guid traceId)
        {
            Measurement measurement = await GetLastTraceMeasurement(traceId);
            return CreateVm(measurement);
        }

        // Trace statistics
        // Server
        public TraceStateModel CreateVm(Measurement measurement)
        {
            var vm = new TraceStateModel();
            PrepareHeader(measurement.TraceId, ref vm);

            vm.TraceId = measurement.TraceId;
            vm.TraceState = measurement.TraceState;
            vm.BaseRefType = measurement.BaseRefType;
            vm.MeasurementTimestamp = measurement.MeasurementTimestamp;
            vm.SorFileId = measurement.SorFileId;

            vm.EventStatus = measurement.EventStatus;
            vm.Comment = measurement.Comment;

            return vm;
        }

        public TraceStateModel CreateVm(OpticalEventModel opticalEventModel)
        {
            var vm = new TraceStateModel();
            PrepareHeader(opticalEventModel.TraceId, ref vm);

            vm.TraceId = opticalEventModel.TraceId;
            vm.TraceState = opticalEventModel.TraceState;
            vm.BaseRefType = opticalEventModel.BaseRefType;
            vm.MeasurementTimestamp = opticalEventModel.MeasurementTimestamp;
            vm.SorFileId = opticalEventModel.SorFileId;

            vm.EventStatus = opticalEventModel.EventStatus;
            vm.Comment = opticalEventModel.Comment;

           return vm;

        }

        private void PrepareHeader(Guid traceId, ref TraceStateModel result)
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