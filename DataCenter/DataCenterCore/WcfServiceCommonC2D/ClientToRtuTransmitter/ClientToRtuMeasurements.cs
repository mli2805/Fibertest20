using System;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.DataCenterCore
{
    public partial class ClientToRtuTransmitter
    {
        public async Task<ClientMeasurementStartedDto> DoClientMeasurementAsync(DoClientMeasurementDto dto)
        {
            _logFile.AppendLine($"Client {_clientsCollection.Get(dto.ConnectionId)} asked to do measurement on RTU {dto.RtuId.First6()}");
            var username = _clientsCollection.Clients.FirstOrDefault(u=>u.ConnectionId == dto.ConnectionId)?.UserName ?? "unknown user";
            var occupation = dto.IsForAutoBase ? RtuOccupation.AutoBaseMeasurement : RtuOccupation.MeasurementClient;
            if (!_rtuOccupations.TrySetOccupation(dto.RtuId, occupation, username, out RtuOccupationState currentState))
            {
                return new ClientMeasurementStartedDto()
                {
                    ReturnCode = ReturnCode.RtuIsBusy,
                    RtuOccupationState = currentState,
                    ErrorMessage = username,
                };
            }
            
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(dto);
            _logFile.AppendLine(json);
            
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
            _logFile.AppendLine($"Client {_clientsCollection.Get(dto.ConnectionId)} asked to do out of turn measurement on RTU {dto.RtuId.First6()}");
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

        public async Task<RequestAnswer> InterruptMeasurementAsync(InterruptMeasurementDto dto)
        {
            _logFile.AppendLine($"Client {_clientsCollection.Get(dto.ConnectionId)} asked to interrupt measurement on RTU {dto.RtuId.First6()}");
            try
            {
                var rtuAddresses = await _rtuStationsRepository.GetRtuAddresses(dto.RtuId);
                if (rtuAddresses != null)
                    return await _d2RWcfManager.SetRtuAddresses(rtuAddresses, _iniFile, _logFile)
                        .InterruptMeasurementAsync(dto);

                _logFile.AppendLine($"Unknown RTU {dto.RtuId.First6()}");
                return new RequestAnswer() { ReturnCode = ReturnCode.DbError };
            }
            catch (Exception e)
            {
                _logFile.AppendLine("InterruptMeasurementAsync:" + e.Message);
                return new RequestAnswer() { ReturnCode = ReturnCode.DbError, ErrorMessage = e.Message };
            }
        }

        public async Task<RequestAnswer> FreeOtdrAsync(FreeOtdrDto dto)
        {
            _logFile.AppendLine($"Client {_clientsCollection.Get(dto.ConnectionId)} asked to free OTDR on RTU {dto.RtuId.First6()}");
            var username = _clientsCollection.Clients.FirstOrDefault(u=>u.ConnectionId == dto.ConnectionId)?.UserName ?? "unknown user";
            if (!_rtuOccupations.TrySetOccupation(dto.RtuId, RtuOccupation.None, username, out RtuOccupationState currentState))
            {
                return new RequestAnswer()
                {
                    ReturnCode = ReturnCode.RtuIsBusy,
                    RtuOccupationState = currentState,
                    ErrorMessage = username,
                };
            }
            try
            {
                var rtuAddresses = await _rtuStationsRepository.GetRtuAddresses(dto.RtuId);
                if (rtuAddresses != null)
                {
                    var res = await _d2RWcfManager.SetRtuAddresses(rtuAddresses, _iniFile, _logFile)
                        .FreeOtdrAsync(dto);
                    _logFile.AppendLine($"FreeOtdrAsync result is {res.ReturnCode}");
                    return res;
                }

                _logFile.AppendLine($"Unknown RTU {dto.RtuId.First6()}");
                return new RequestAnswer() { ReturnCode = ReturnCode.DbError };
            }
            catch (Exception e)
            {
                _logFile.AppendLine("FreeOtdrAsync:" + e.Message);
                return new RequestAnswer() { ReturnCode = ReturnCode.DbError, ErrorMessage = e.Message };
            }
        }

    }
}
