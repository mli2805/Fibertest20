using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.DataCenterCore
{
    public partial class WcfIntermediateC2R
    {
        public async Task<OtauAttachedDto> AttachOtauAsync(AttachOtauDto dto)
        {
            var rtuAddresses = await _rtuStationsRepository.GetRtuAddresses(dto.RtuId);
            if (rtuAddresses == null)
            {
                _logFile.AppendLine($"Unknown RTU {dto.RtuId.First6()}");
                return new OtauAttachedDto(ReturnCode.NoSuchRtu)
                {
                    ErrorMessage = $"Unknown RTU {dto.RtuId.First6()}"
                };
            }

            OtauAttachedDto result;
            switch (rtuAddresses.Main.Port)
            {
                case (int)TcpPorts.RtuListenTo:
                    result = await _clientToRtuTransmitter.AttachOtauAsync(dto, rtuAddresses); break;
                case (int)TcpPorts.RtuVeexListenTo:
                    result = await _clientToRtuVeexTransmitter.AttachOtauAsync(dto, rtuAddresses); break;
                case (int)TcpPorts.RtuListenToHttp:
                    result = await _clientToLinuxRtuHttpTransmitter.AttachOtauAsync(dto, rtuAddresses); break;
                default:
                    return new OtauAttachedDto(ReturnCode.Error);
            }

            return result;
        }

        public async Task<OtauDetachedDto> DetachOtauAsync(DetachOtauDto dto)
        {
            var rtuAddresses = await _rtuStationsRepository.GetRtuAddresses(dto.RtuId);
            if (rtuAddresses == null)
            {
                _logFile.AppendLine($"Unknown RTU {dto.RtuId.First6()}");
                return new OtauDetachedDto(ReturnCode.NoSuchRtu)
                {
                    ErrorMessage = $"Unknown RTU {dto.RtuId.First6()}"
                };
            }

            OtauDetachedDto result;
            switch (rtuAddresses.Main.Port)
            {
                case (int)TcpPorts.RtuListenTo:
                    result = await _clientToRtuTransmitter.DetachOtauAsync(dto, rtuAddresses); break;
                case (int)TcpPorts.RtuVeexListenTo:
                    result = await _clientToRtuVeexTransmitter.DetachOtauAsync(dto, rtuAddresses); break;
                case (int)TcpPorts.RtuListenToHttp:
                    result = await _clientToLinuxRtuHttpTransmitter.DetachOtauAsync(dto, rtuAddresses); break;
                default:
                    return new OtauDetachedDto(ReturnCode.Error);
            }

            return result;
        }
    }
}
