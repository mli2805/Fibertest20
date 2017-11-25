using System;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class TraceStateManager
    {
        private readonly IMyLog _logFile;
        private readonly ReadModel _readModel;
        private readonly IWindowManager _windowManager;
        private readonly IWcfServiceForClient _c2DWcfManager;

        public TraceStateManager(IMyLog logFile, ReadModel readModel, IWindowManager windowManager, IWcfServiceForClient c2DWcfManager)
        {
            _logFile = logFile;
            _readModel = readModel;
            _windowManager = windowManager;
            _c2DWcfManager = c2DWcfManager;
        }

        // from TraceLeaf
        public void ShowTraceState(Guid traceId)
        {
            ShowTraceState(Prepare(traceId));

        }

        // from TraceStatistics
        public void ShowTraceState(MeasurementVm measurementVm)
        {
            ShowTraceState(Prepare(measurementVm));
        }

        // from OpticalEvents
        public void ShowTraceState(OpticalEventVm opticalEventVm)
        {
            ShowTraceState(Prepare(opticalEventVm));
        }

        // from Accident happend
        public void ShowTraceState(MonitoringResultDto monitoringResultDto)
        {
            ShowTraceState(Prepare(monitoringResultDto));
        }

        //----------------------------------------------------

        private void ShowTraceState(TraceStateVm traceStateVm)
        {
            var vm = new TraceStateViewModel();
            vm.Initialize(traceStateVm);
            _windowManager.ShowDialog(vm);
        }

        private TraceStateVm Prepare(Guid traceId)
        {
            var result = new TraceStateVm();
            PrepareCaption(traceId, ref result);

            TraceStateDto dto = GetLastTraceState(traceId).Result;

            return result;
        }

        private TraceStateVm Prepare(MeasurementVm measurementVm)
        {
            return new TraceStateVm()
            {
                BaseRefType = measurementVm.BaseRefType,
                TraceState = measurementVm.TraceState,
            };
        }

        private TraceStateVm Prepare(OpticalEventVm opticalEventVm)
        {
            return new TraceStateVm()
            {
                TraceState = opticalEventVm.TraceState,
            };
        }

        private TraceStateVm Prepare(MonitoringResultDto monitoringResultDto)
        {
            return new TraceStateVm()
            {
                TraceState = monitoringResultDto.TraceState,
            };
        }

        //-----------------------------
        private void PrepareCaption(Guid traceId, ref TraceStateVm result)
        {
            var trace = _readModel.Traces.FirstOrDefault(t => t.Id == traceId);
            if (trace == null)
                return;

            result.TraceTitle = trace.Title;
            result.RtuTitle = _readModel.Rtus.FirstOrDefault(r => r.Id == trace.RtuId)?.Title;
            result.PortTitle = trace.OtauPort.IsPortOnMainCharon
                ? trace.OtauPort.OpticalPort.ToString()
                : $@"{trace.OtauPort.OtauIp}:{trace.OtauPort.OtauTcpPort}-{trace.OtauPort.OpticalPort}";
        }

        private async Task<TraceStateDto> GetLastTraceState(Guid traceId)
        {
            var traceStateDto = await _c2DWcfManager.GetLastTraceState(traceId);
            if (traceStateDto == null)
            {
                _logFile.AppendLine($@"Cannot get last state for trace {traceId.First6()}");
                return null;
            }
            else
            {
                _logFile.AppendLine($@"Last state for trace {traceId.First6()} recieved");
                return traceStateDto;
            }
        }

    }
}