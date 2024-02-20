using Autofac;
using Iit.Fibertest.Dto;
using System.Threading.Tasks;
using System;

namespace Iit.Fibertest.DataCenterCore
{
    public partial class WcfIntermediateC2R
    {
            public async Task<RtuInitializedDto> InitializeRtuAsync(InitializeRtuDto dto)
        {
            var clientStation = _clientsCollection.Get(dto.ConnectionId);
            _logFile.AppendLine($"Client {clientStation} sent initialize RTU {dto.RtuId.First6()} request");

            if (!_rtuOccupations.TrySetOccupation(dto.RtuId, RtuOccupation.Initialization,
                    clientStation?.UserName, out RtuOccupationState currentState))
            {
                return new RtuInitializedDto()
                {
                    RtuId = dto.RtuId,
                    IsInitialized = false,
                    ReturnCode = ReturnCode.RtuIsBusy,
                    RtuOccupationState = currentState,
                };
            }

            dto.ServerAddresses = (DoubleAddress)_serverDoubleAddress.Clone();
            if (!dto.RtuAddresses.HasReserveAddress)
                // if RTU has no reserve address it should not send to server's reserve address
                // (it is an ideological requirement)
                dto.ServerAddresses.HasReserveAddress = false;

            RtuInitializedDto rtuInitializedDto;
            switch (dto.RtuAddresses.Main.Port)
            {
                case (int)TcpPorts.RtuListenTo:
                    rtuInitializedDto = await _clientToRtuTransmitter.InitializeRtuAsync(dto); break;
                case (int)TcpPorts.RtuVeexListenTo:
                    rtuInitializedDto = await _clientToRtuVeexTransmitter.InitializeRtuAsync(dto); break;
                case (int)TcpPorts.RtuListenToHttp:
                    rtuInitializedDto = await _clientToLinuxRtuHttpTransmitter.InitializeRtuAsync(dto); break;
                default:
                    return new RtuInitializedDto(ReturnCode.Error);
            }

            if (rtuInitializedDto.ReturnCode == ReturnCode.InProgress &&
                dto.RtuAddresses.Main.Port == (int)TcpPorts.RtuListenToHttp)
            {
                rtuInitializedDto = await PollMakLinuxForInitializationResult(dto.RtuAddresses);
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


            _rtuOccupations.TrySetOccupation(dto.RtuId, RtuOccupation.None, clientStation?.UserName, out RtuOccupationState _);

            var rtuInitializationToGraphApplier = _globalScope.Resolve<RtuInitializationToGraphApplier>();
            return await rtuInitializationToGraphApplier.ApplyRtuInitializationResult(dto, rtuInitializedDto);
        }

        private async Task<RtuInitializedDto> PollMakLinuxForInitializationResult(DoubleAddress rtuDoubleAddress)
        {
            var count = 18; // 18 * 5 sec = 90 sec limit
            var requestDto = new GetCurrentRtuStateDto() { RtuDoubleAddress = rtuDoubleAddress };
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
