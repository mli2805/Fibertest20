using System;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.DataCenterCore
{
    public partial class ClientToRtuTransmitter
    {
        public async Task<ClientMeasurementStartedDto> DoClientMeasurementAsync(DoClientMeasurementDto dto, DoubleAddress rtuDoubleAddress)
        {
            try
            {
                var result = await _d2RWcfManager
                    .SetRtuAddresses(rtuDoubleAddress, _iniFile, _logFile)
                    .DoClientMeasurementAsync(dto);
                _logFile.AppendLine($"Client's measurement started with code {result.ReturnCode.ToString()}");
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("StartClientMeasurementAsync:" + e.Message);
                return new ClientMeasurementStartedDto() { ReturnCode = ReturnCode.Error, ErrorMessage = e.Message };
            }
        }

        public async Task<RequestAnswer> DoOutOfTurnPreciseMeasurementAsync(DoOutOfTurnPreciseMeasurementDto dto, DoubleAddress rtuDoubleAddress)
        {
            _logFile.AppendLine($"Client {_clientsCollection.Get(dto.ConnectionId)} asked to do out of turn measurement on RTU {dto.RtuId.First6()}");
            try
            {
                return await _d2RWcfManager.SetRtuAddresses(rtuDoubleAddress, _iniFile, _logFile)
                    .DoOutOfTurnPreciseMeasurementAsync(dto);
            }
            catch (Exception e)
            {
                _logFile.AppendLine("DoOutOfTurnPreciseMeasurementAsync:" + e.Message);
                return new RequestAnswer() { ReturnCode = ReturnCode.DbError, ErrorMessage = e.Message };
            }
        }

        public async Task<RequestAnswer> InterruptMeasurementAsync(InterruptMeasurementDto dto, DoubleAddress rtuDoubleAddress)
        {
            try
            {
                return await _d2RWcfManager.SetRtuAddresses(rtuDoubleAddress, _iniFile, _logFile)
                    .InterruptMeasurementAsync(dto);
            }
            catch (Exception e)
            {
                _logFile.AppendLine("InterruptMeasurementAsync:" + e.Message);
                return new RequestAnswer() { ReturnCode = ReturnCode.DbError, ErrorMessage = e.Message };
            }
        }

        public async Task<RequestAnswer> FreeOtdrAsync(FreeOtdrDto dto, DoubleAddress rtuDoubleAddress)
        {
            try
            {
                var res = await _d2RWcfManager.SetRtuAddresses(rtuDoubleAddress, _iniFile, _logFile)
                    .FreeOtdrAsync(dto);
                _logFile.AppendLine($"FreeOtdrAsync result is {res.ReturnCode}");
                return res;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("FreeOtdrAsync:" + e.Message);
                return new RequestAnswer() { ReturnCode = ReturnCode.DbError, ErrorMessage = e.Message };
            }
        }

    }
}
