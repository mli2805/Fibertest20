using System;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.DataCenterCore
{
    public partial class ClientToRtuTransmitter
    {
        public async Task<ClientMeasurementStartedDto> DoClientMeasurementAsync(DoClientMeasurementDto dto)
        {
            _logFile.AppendLine($"Client {dto.ConnectionId} / {dto.ClientIp} asked to do measurement on RTU {dto.RtuId.First6()}");
            try
            {
                var rtuAddresses = await _rtuStationsRepository.GetRtuAddresses(dto.RtuId);
                if (rtuAddresses != null)
                {
                    var result = await _d2RWcfManager
                        .SetRtuAddresses(rtuAddresses, _iniFile, _logFile)
                        .DoClientMeasurementAsync(dto);
                    _logFile.AppendLine($"Client's measurement started with code {result.ReturnCode.ToString()}");
                    return result;
                }

                _logFile.AppendLine($"Unknown RTU {dto.RtuId.First6()}");
                return new ClientMeasurementStartedDto() { ReturnCode = ReturnCode.DbError };
            }
            catch (Exception e)
            {
                _logFile.AppendLine("DoClientMeasurementAsync:" + e.Message);
                return new ClientMeasurementStartedDto() { ReturnCode = ReturnCode.DbError, ErrorMessage = e.Message };
            }
        }

        public async Task<RequestAnswer> DoOutOfTurnPreciseMeasurementAsync(DoOutOfTurnPreciseMeasurementDto dto)
        {
            _logFile.AppendLine($"Client from {dto.ClientIp} asked to do out of turn measurement on RTU {dto.RtuId.First6()}");
            try
            {
                var rtuAddresses = await _rtuStationsRepository.GetRtuAddresses(dto.RtuId);
                if (rtuAddresses != null)
                    return await _d2RWcfManager.SetRtuAddresses(rtuAddresses, _iniFile, _logFile)
                        .DoOutOfTurnPreciseMeasurementAsync(dto);

                _logFile.AppendLine($"Unknown RTU {dto.RtuId.First6()}");
                return new RequestAnswer() { ReturnCode = ReturnCode.DbError };
            }
            catch (Exception e)
            {
                _logFile.AppendLine("DoOutOfTurnPreciseMeasurementAsync:" + e.Message);
                return new RequestAnswer() { ReturnCode = ReturnCode.DbError, ErrorMessage = e.Message };
            }
        }

    }
}
