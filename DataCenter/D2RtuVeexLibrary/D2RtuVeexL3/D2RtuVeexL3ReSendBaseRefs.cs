using System;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public partial class D2RtuVeexLayer3
    {
        public async Task<BaseRefAssignedDto> ReSendBaseRefsAsync(ReSendBaseRefsDto dto, DoubleAddress rtuAddresses)
        {
            try
            {
                BaseRefAssignedDto createResult = new BaseRefAssignedDto();
                foreach (var baseRefDto in dto.BaseRefDtos)
                {
                    createResult =
                        await _d2RtuVeexLayer21.ReSetBaseRefs(rtuAddresses, dto.OtdrId, dto.OtauPortDto.OtauId,
                            dto.OtauPortDto.OpticalPort - 1, baseRefDto);
                    if (createResult.ReturnCode != ReturnCode.BaseRefAssignedSuccessfully)
                        return createResult;
                }

                return createResult;
            }
            catch (Exception e)
            {
                return new BaseRefAssignedDto()
                {
                    ReturnCode = ReturnCode.BaseRefAssignmentFailed,
                    ErrorMessage = e.Message
                };
            }
        }
    }
}
