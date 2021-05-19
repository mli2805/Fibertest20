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
                var oldTests = await _d2RtuVeexLayer2.GetTestsForPort(rtuAddresses, dto.OtauPortDto.OpticalPort);
                foreach (var oldTest in oldTests)
                {
                    if (!await _d2RtuVeexLayer1.DeleteTest(rtuAddresses, $@"tests/{oldTest.id}"))
                        return new BaseRefAssignedDto(){ ReturnCode = ReturnCode.BaseRefAssignmentFailed };
                }

                BaseRefAssignedDto createResult = new BaseRefAssignedDto();
                foreach (var baseRefDto in dto.BaseRefDtos)
                {
                    createResult =
                        await _d2RtuVeexLayer21.FullTestCreation(rtuAddresses, dto.OtdrId,
                            dto.OtauPortDto.OpticalPort - 1, baseRefDto);
                }


                return createResult;
            }
            catch (Exception)
            {
                return new BaseRefAssignedDto(){ ReturnCode = ReturnCode.BaseRefAssignmentFailed };
            }
        }
    }
}
