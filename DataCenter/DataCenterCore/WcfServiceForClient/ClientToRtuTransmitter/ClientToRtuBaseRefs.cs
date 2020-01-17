using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.DataCenterCore
{
    public partial class ClientToRtuTransmitter
    {
        // Base refs had been assigned earlier (and saved in Db) and now user attached trace to the port
        // base refs should be extracted from Db and sent to the RTU
        // or user explicitly demands to resend base refs to RTU 
        public async Task<BaseRefAssignedDto> ReSendBaseRefAsync(ReSendBaseRefsDto dto)
        {
            _logFile.AppendLine($"Client {dto.ClientId.First6()} asked to re-send base ref for trace {dto.TraceId.First6()}");

            var convertedDto = await ConvertToAssignBaseRefsDto(dto);

            if (convertedDto?.BaseRefs == null)
                return new BaseRefAssignedDto() { ReturnCode = ReturnCode.DbCannotConvertThisReSendToAssign };
            if (!convertedDto.BaseRefs.Any())
                return new BaseRefAssignedDto() { ReturnCode = ReturnCode.BaseRefAssignedSuccessfully };

            return await TransmitBaseRefsToRtu(convertedDto);
        }

        private async Task<AssignBaseRefsDto> ConvertToAssignBaseRefsDto(ReSendBaseRefsDto dto)
        {
            var result = new AssignBaseRefsDto()
            {
                ClientId = dto.ClientId,
                RtuId = dto.RtuId,
                TraceId = dto.TraceId,
                OtauPortDto = dto.OtauPortDto,
                BaseRefs = new List<BaseRefDto>(),
            };

            foreach (var baseRefDto in dto.BaseRefDtos)
            {
                baseRefDto.SorBytes = await _sorFileRepository.GetSorBytesAsync(baseRefDto.SorFileId);
                result.BaseRefs.Add(baseRefDto);
            }

            return result;
        }


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
                return new BaseRefAssignedDto() { ReturnCode = ReturnCode.DbError, ExceptionMessage = "RTU's address not found in Db"};
            }
            catch (Exception e)
            {
                _logFile.AppendLine("AssignBaseRefAsync: " + e.Message);
                return new BaseRefAssignedDto() { ReturnCode = ReturnCode.DbError, ExceptionMessage = e.Message };
            }
        }

      
       
    }
}
