using System;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class TraceStateManager
    {
        private readonly IMyLog _logFile;
        private readonly ReadModel _readModel;
        private readonly IMyWindowManager _windowManager;
        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly TraceStateViewModel _traceStateViewModel;

        public TraceStateManager(IMyLog logFile, ReadModel readModel, 
            IMyWindowManager windowManager, IWcfServiceForClient c2DWcfManager,
            TraceStateViewModel traceStateViewModel)
        {
            _logFile = logFile;
            _readModel = readModel;
            _windowManager = windowManager;
            _c2DWcfManager = c2DWcfManager;
            _traceStateViewModel = traceStateViewModel;
        }

        // from TraceLeaf
        public void ShowTraceState(Guid traceId)
        {
            ShowTraceState(Prepare(traceId));
        }

        // from TraceStatistics
        public void ShowTraceState(Guid traceId, MeasurementVm measurementVm)
        {
            ShowTraceState(Prepare(traceId, measurementVm));
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
            _traceStateViewModel.Initialize(traceStateVm);
            _windowManager.ShowWindow(_traceStateViewModel);
        }

        private TraceStateVm Prepare(Guid traceId)
        {
            var result = new TraceStateVm();
            PrepareCaption(traceId, ref result);

            TraceStateDto dto = GetLastTraceState(traceId).Result;

            result.TraceState = dto.LastMeasurement.TraceState;
            result.BaseRefType = dto.LastMeasurement.BaseRefType;
            result.SorFileId = dto.LastMeasurement.SorFileId;

            if (dto.CorrespondentEvent != null)
            {
                result.OpticalEventId = dto.CorrespondentEvent.Id;
                result.EventStatus = dto.CorrespondentEvent.EventStatus;
                result.OpticalEventComment = dto.CorrespondentEvent.Comment;
            }
            return result;
        }

        private TraceStateVm Prepare(Guid traceId, MeasurementVm measurementVm)
        {
            var result = new TraceStateVm();
            PrepareCaption(traceId, ref result);

            result.TraceState = measurementVm.TraceState;
            result.BaseRefType = measurementVm.BaseRefType;
            result.SorFileId = measurementVm.SorFileId;

            return result;
        }

        private TraceStateVm Prepare(OpticalEventVm opticalEventVm)
        {
            var result = new TraceStateVm();
            PrepareCaption(opticalEventVm.TraceId, ref result);

            result.TraceState = opticalEventVm.TraceState;
            result.BaseRefType = opticalEventVm.BaseRefType;
            result.SorFileId = opticalEventVm.SorFileId;

            result.OpticalEventId = opticalEventVm.Nomer;
            result.EventStatus = opticalEventVm.EventStatus;
            result.OpticalEventComment = opticalEventVm.Comment;

            return result;

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
            result.PortTitle = trace.OtauPort == null ? "<not attached>" : trace.OtauPort.IsPortOnMainCharon
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