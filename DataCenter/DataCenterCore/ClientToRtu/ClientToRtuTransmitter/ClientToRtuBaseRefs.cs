using System;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.DataCenterCore
{
    public partial class ClientToRtuTransmitter
    {
        
        public async Task<BaseRefAssignedDto> TransmitBaseRefsToRtuAsync(AssignBaseRefsDto dto, DoubleAddress rtuDoubleAddress)
        {
            try
            {
                var result = await _d2RWcfManager.SetRtuAddresses(rtuDoubleAddress, _iniFile, _logFile)
                    .AssignBaseRefAsync(dto);
                _logFile.AppendLine($"Assign base ref(s) result is {result.ReturnCode}");
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("AssignBaseRefAsync: " + e.Message);
                return new BaseRefAssignedDto() { ReturnCode = ReturnCode.DbError, ErrorMessage = e.Message };
            }
        }

        // Veex only
        public Task<ClientMeasurementVeexResultDto> GetMeasurementClientResultAsync(GetClientMeasurementDto dto)
        {
            throw new NotImplementedException();
        }

        public Task<ClientMeasurementVeexResultDto> GetClientMeasurementSorBytesAsync(GetClientMeasurementDto dto)
        {
            throw new NotImplementedException();
        }
    }
}
