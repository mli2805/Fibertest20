using System;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
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

        public TraceStateVm Create(Guid traceId)
        {
            var vm = new TraceStateVm();
            PrepareCaption(traceId, ref vm);

            Measurement dto = GetLastTraceMeasurement(traceId).Result;

            vm.TraceState = dto.TraceState;
            vm.BaseRefType = dto.BaseRefType;
            vm.SorFileId = dto.SorFileId;

            vm.EventStatus = dto.EventStatus;
            vm.OpticalEventComment = dto.Comment;

            return vm;
        }

        public TraceStateVm Create(Guid traceId, MeasurementVm measurementVm)
        {
            var vm = new TraceStateVm();
            PrepareCaption(traceId, ref vm);

            vm.TraceState = measurementVm.TraceState;
            vm.BaseRefType = measurementVm.BaseRefType;
            vm.SorFileId = measurementVm.SorFileId;

            return vm;
        }

        public TraceStateVm Create(Measurement measurement)
        {
            var vm = new TraceStateVm();
            PrepareCaption(measurement.TraceId, ref vm);

            vm.TraceState = measurement.TraceState;
            vm.BaseRefType = measurement.BaseRefType;
            vm.SorFileId = measurement.SorFileId;

            return vm;
        }

        public TraceStateVm Create(OpticalEventVm opticalEventVm)
        {
            var vm = new TraceStateVm();
            PrepareCaption(opticalEventVm.TraceId, ref vm);

            vm.TraceState = opticalEventVm.TraceState;
            vm.BaseRefType = opticalEventVm.BaseRefType;
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
            result.PortTitle = trace.OtauPort == null ? "<not attached>" : trace.OtauPort.IsPortOnMainCharon
                ? trace.OtauPort.OpticalPort.ToString()
                : $@"{trace.OtauPort.OtauIp}:{trace.OtauPort.OtauTcpPort}-{trace.OtauPort.OpticalPort}";
        }

        private async Task<Measurement> GetLastTraceMeasurement(Guid traceId)
        {
            var measurement = await _c2DWcfManager.GetLastTraceMeasurement(traceId);
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