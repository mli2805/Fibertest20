using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.DataCenterCore
{
    public partial class WcfIntermediateC2R
    {
        public async Task<ClientMeasurementStartedDto> DoClientMeasurementAsync(DoClientMeasurementDto dto)
        {
            var rtuAddresses = await _rtuStationsRepository.GetRtuAddresses(dto.RtuId);
            if (rtuAddresses == null)
            {
                _logFile.AppendLine($"Unknown RTU {dto.RtuId.First6()}");
                return new ClientMeasurementStartedDto(ReturnCode.NoSuchRtu)
                {
                    ErrorMessage = $"Unknown RTU {dto.RtuId.First6()}"
                };
            }
            
            switch (rtuAddresses.Main.Port)
            {
                case (int)TcpPorts.RtuListenTo:
                    return await _clientToRtuTransmitter.DoClientMeasurementAsync(dto, rtuAddresses);
                case (int)TcpPorts.RtuVeexListenTo:
                    return await _clientToRtuVeexTransmitter.DoClientMeasurementAsync(dto, rtuAddresses);
                case (int)TcpPorts.RtuListenToHttp:
                    return await _clientToLinuxRtuHttpTransmitter.DoClientMeasurementAsync(dto, rtuAddresses);
                default:
                    return new ClientMeasurementStartedDto(ReturnCode.Error);
            }
        }

        public async Task<RequestAnswer> DoOutOfTurnPreciseMeasurementAsync(DoOutOfTurnPreciseMeasurementDto dto)
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

            RequestAnswer result;
            switch (rtuAddresses.Main.Port)
            {
                case (int)TcpPorts.RtuListenTo:
                    result = await _clientToRtuTransmitter.DoOutOfTurnPreciseMeasurementAsync(dto, rtuAddresses); break;
                case (int)TcpPorts.RtuVeexListenTo:
                    result = await _clientToRtuVeexTransmitter.DoOutOfTurnPreciseMeasurementAsync(dto, rtuAddresses); break;
                case (int)TcpPorts.RtuListenToHttp:
                    result = await _clientToLinuxRtuHttpTransmitter.DoOutOfTurnPreciseMeasurementAsync(dto, rtuAddresses); break;
                default:
                    return new OtauDetachedDto(ReturnCode.Error);
            }

            return result;
        }
    }
}
