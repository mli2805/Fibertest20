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
                        await _d2RtuVeexLayer21.ReSetBaseRefs(rtuAddresses, dto.OtdrId,
                            dto.OtauPortDto.OpticalPort - 1, baseRefDto);
                }

                return createResult;
            }
            catch (Exception)
            {
                return new BaseRefAssignedDto() { ReturnCode = ReturnCode.BaseRefAssignmentFailed };
            }
        }
    }
}
