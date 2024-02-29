using Iit.Fibertest.Dto;
using System.Threading.Tasks;
using System;

namespace Iit.Fibertest.DataCenterCore
{
    public partial class WcfIntermediateC2R
    {
        public async Task<RtuInitializedDto> InitializeRtuAsync(InitializeRtuDto dto)
        {
            if (!TryToGetClientAndOccupyRtu(dto.ConnectionId, dto.RtuId, RtuOccupation.Initialization,
                    out RtuInitializedDto response))
                return response;

            dto.ServerAddresses = (DoubleAddress)_serverDoubleAddress.Clone();
            if (!dto.RtuAddresses.HasReserveAddress)
                // if RTU has no reserve address it should not send to server's reserve address
                // (it is an ideological requirement)
                dto.ServerAddresses.HasReserveAddress = false;

            var rtuInitializedDto = await GetRtuSpecificTransmitter(dto.RtuAddresses.Main.Port).InitializeRtuAsync(dto);

            if (rtuInitializedDto.ReturnCode == ReturnCode.InProgress &&
                dto.RtuAddresses.Main.Port == (int)TcpPorts.RtuListenToHttp)
            {
                rtuInitializedDto = await PollMakLinuxForInitializationResult(dto.RtuId, dto.RtuAddresses);
            }

            await _ftSignalRClient.NotifyAll("RtuInitialized", rtuInitializedDto.ToCamelCaseJson());

            if (rtuInitializedDto.IsInitialized)
            {
                try
                {
                    rtuInitializedDto.RtuAddresses = dto.RtuAddresses;
                    var rtuStation = RtuStationFactory.Create(rtuInitializedDto);
                    await _rtuStationsRepository.RegisterRtuInitializationResultAsync(rtuStation);
                }
                catch (Exception e)
                {
                    rtuInitializedDto.ReturnCode = ReturnCode.Error;
                    rtuInitializedDto.ErrorMessage = $"Failed to save RTU in DB: {e.Message}";
                }
            }

            var message = rtuInitializedDto.IsInitialized
                ? "RTU initialized successfully, monitoring mode is " +
                  (rtuInitializedDto.IsMonitoringOn ? "AUTO" : "MANUAL")
                : "RTU initialization failed";
            _logFile.AppendLine(message);


            _rtuOccupations.TrySetOccupation(dto.RtuId, RtuOccupation.None, response.UserName, out RtuOccupationState _);

            return await ApplyRtuInitializationResult(dto, rtuInitializedDto);
        }

        private async Task<RtuInitializedDto> PollMakLinuxForInitializationResult(Guid rtuId, DoubleAddress rtuDoubleAddress)
        {
            var count = 18; // 18 * 5 sec = 90 sec limit
            var requestDto = new GetCurrentRtuStateDto() { RtuId = rtuId, RtuDoubleAddress = rtuDoubleAddress };
            while (--count >= 0)
            {
                await Task.Delay(5000);
                var state = await _clientToLinuxRtuHttpTransmitter.GetRtuCurrentState(requestDto);
                if (state.LastInitializationResult != null)
                    return state.LastInitializationResult.Result;
            }

            return new RtuInitializedDto(ReturnCode.TimeOutExpired);
        }

    }
}
