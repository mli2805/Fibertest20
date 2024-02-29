using System;
using Iit.Fibertest.Dto;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.DataCenterCore
{
    public partial class WcfIntermediateC2R
    {
        public async Task<RequestAnswer> ApplyMonitoringSettingsAsync(ApplyMonitoringSettingsDto dto)
        {
            if (!TryToGetClientAndOccupyRtu(dto.ConnectionId, dto.RtuId, RtuOccupation.MonitoringSettings,
                    out RequestAnswer response))
                return response;

            var rtuAddresses = await _rtuStationsRepository.GetRtuAddresses(dto.RtuId);
            if (rtuAddresses == null)
                return new RequestAnswer(ReturnCode.NoSuchRtu);
            
            foreach (var portWithTraceDto in dto.Ports)
            {
                var trace = _writeModel.Traces.FirstOrDefault(t => t.TraceId == portWithTraceDto.TraceId);
                portWithTraceDto.LastTraceState = trace?.State ?? FiberState.Unknown;
            }

            var applyResult = await GetRtuSpecificTransmitter(rtuAddresses.Main.Port)
                .ApplyMonitoringSettingsAsync(dto, rtuAddresses);

            if (applyResult.ReturnCode == ReturnCode.InProgress &&
                rtuAddresses.Main.Port == (int)TcpPorts.RtuListenToHttp)
            {
                applyResult = await PollMakLinuxForApplyMonitoringSettingsResult(dto.RtuId, rtuAddresses);
            }

            if (applyResult.ReturnCode == ReturnCode.MonitoringSettingsAppliedSuccessfully)
            {
                var cmd = new ChangeMonitoringSettings
                {
                    RtuId = dto.RtuId,
                    PreciseMeas = dto.Timespans.PreciseMeas.GetFrequency(),
                    PreciseSave = dto.Timespans.PreciseSave.GetFrequency(),
                    FastSave = dto.Timespans.FastSave.GetFrequency(),
                    TracesInMonitoringCycle = dto.Ports.Select(p => p.TraceId).ToList(),
                    IsMonitoringOn = dto.IsMonitoringOn,
                };

                var resultFromEventStore = await _eventStoreService.SendCommand(cmd, response.UserName, dto.ClientIp);

                if (!string.IsNullOrEmpty(resultFromEventStore))
                {
                    return new RequestAnswer
                    {
                        ReturnCode = ReturnCode.RtuMonitoringSettingsApplyError,
                        ErrorMessage = resultFromEventStore
                    };
                }
                else
                {
                    if (dto.IsMonitoringOn)
                        await _ftSignalRClient.NotifyAll("MonitoringStarted", $"{{\"rtuId\" : \"{dto.RtuId}\"}}");
                    else
                        await _ftSignalRClient.NotifyAll("MonitoringStopped", $"{{\"rtuId\" : \"{dto.RtuId}\"}}");
                }
            }

            _rtuOccupations.TrySetOccupation(dto.RtuId, RtuOccupation.None, response.UserName, out RtuOccupationState _);

            return applyResult;
        }

        private async Task<RequestAnswer> PollMakLinuxForApplyMonitoringSettingsResult(Guid rtuId, DoubleAddress rtuDoubleAddress)
        {
            var count = 18; // 18 * 5 sec = 90 sec limit
            var requestDto = new GetCurrentRtuStateDto() { RtuId = rtuId, RtuDoubleAddress = rtuDoubleAddress };
            while (--count >= 0)
            {
                await Task.Delay(5000);
                var state = await _clientToLinuxRtuHttpTransmitter.GetRtuCurrentState(requestDto);
                if (state.ReturnCode == ReturnCode.D2RHttpError)
                    return new RequestAnswer(ReturnCode.FailedToApplyMonitoringSettings);
                if ( state.LastInitializationResult != null)
                    return new RequestAnswer(ReturnCode.MonitoringSettingsAppliedSuccessfully);
            }

            return new RequestAnswer(ReturnCode.TimeOutExpired);
        }
    }
}
