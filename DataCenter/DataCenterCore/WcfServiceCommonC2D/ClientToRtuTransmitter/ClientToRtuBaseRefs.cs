using System;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.DataCenterCore
{
    public partial class ClientToRtuTransmitter
    {
        
        public async Task<BaseRefAssignedDto> TransmitBaseRefsToRtu(AssignBaseRefsDto dto)
        {
            try
            {
                var rtuAddresses = await _rtuStationsRepository.GetRtuAddresses(dto.RtuId);
                if (rtuAddresses != null)
                {
                    var result = await _d2RWcfManager.SetRtuAddresses(rtuAddresses, _iniFile, _logFile)
                        .AssignBaseRefAsync(dto);
                    _logFile.AppendLine($"Assign base ref(s) result is {result.ReturnCode}");
                    return result;
                }

                _logFile.AppendLine($"Unknown RTU {dto.RtuId.First6()}");
                return new BaseRefAssignedDto() { ReturnCode = ReturnCode.DbError, ErrorMessage = "RTU's address not found in Db"};
            }
            catch (Exception e)
            {
                _logFile.AppendLine("AssignBaseRefAsync: " + e.Message);
                return new BaseRefAssignedDto() { ReturnCode = ReturnCode.DbError, ErrorMessage = e.Message };
            }
        }

      
       
    }
}
