using Iit.Fibertest.Dto;
using System.Threading.Tasks;

namespace Iit.Fibertest.DataCenterCore
{
    public partial class WcfIntermediateC2R
    {
        public async Task<RequestAnswer> ApplyMonitoringSettingsAsync(ApplyMonitoringSettingsDto dto)
        {
            var rtuAddresses = await _rtuStationsRepository.GetRtuAddresses(dto.RtuId);
            if (rtuAddresses == null)
            {
                _logFile.AppendLine($"Unknown RTU {dto.RtuId.First6()}");
                return new RequestAnswer(ReturnCode.NoSuchRtu)
                {
                    ErrorMessage = $"Unknown RTU {dto.RtuId.First6()}"
                };
            }

            RequestAnswer applyResult;
            switch (rtuAddresses.Main.Port)
            {
                case (int)TcpPorts.RtuListenTo:
                    applyResult = await _clientToRtuTransmitter.ApplyMonitoringSettingsAsync(dto, rtuAddresses); break;
                case (int)TcpPorts.RtuVeexListenTo:
                    applyResult = await _clientToRtuVeexTransmitter.ApplyMonitoringSettingsAsync(dto, rtuAddresses); break;
                case (int)TcpPorts.RtuListenToHttp:
                    applyResult = await _clientToLinuxRtuHttpTransmitter.ApplyMonitoringSettingsAsync(dto, rtuAddresses); break;
                default:
                    return new RequestAnswer(ReturnCode.Error);
            }

            if (applyResult.ReturnCode == ReturnCode.InProgress &&
                rtuAddresses.Main.Port == (int)TcpPorts.RtuListenToHttp)
            {
                applyResult = await PollMakLinuxForApplyMonitoringSettingsResult(rtuAddresses);
            }
            return applyResult;
        }

        private async Task<RequestAnswer> PollMakLinuxForApplyMonitoringSettingsResult(DoubleAddress rtuDoubleAddress)
        {
            var count = 18; // 18 * 5 sec = 90 sec limit
            var requestDto = new GetCurrentRtuStateDto() { RtuDoubleAddress = rtuDoubleAddress };
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

        public async Task<RequestAnswer> StopMonitoringAsync(StopMonitoringDto dto)
        {
            var rtuAddresses = await _rtuStationsRepository.GetRtuAddresses(dto.RtuId);
            if (rtuAddresses == null)
            {
                _logFile.AppendLine($"Unknown RTU {dto.RtuId.First6()}");
                return new RequestAnswer(ReturnCode.NoSuchRtu)
                {
                    ErrorMessage = $"Unknown RTU {dto.RtuId.First6()}"
                };
            }

            switch (rtuAddresses.Main.Port)
            {
                case (int)TcpPorts.RtuListenTo:
                    return await _clientToRtuTransmitter.StopMonitoringAsync(dto, rtuAddresses);
                case (int)TcpPorts.RtuVeexListenTo:
                    return await _clientToRtuVeexTransmitter.StopMonitoringAsync(dto, rtuAddresses);
                case (int)TcpPorts.RtuListenToHttp:
                    return await _clientToLinuxRtuHttpTransmitter.StopMonitoringAsync(dto, rtuAddresses);
                default:
                    return new RequestAnswer(ReturnCode.Error);
            }
        }

    }
}
