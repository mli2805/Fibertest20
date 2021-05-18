using System;
using System.Net;
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
                var oldTests = await GetTestsForPort(rtuAddresses, dto.OtauPortDto.OpticalPort);
                foreach (var oldTest in oldTests)
                {
                    var delResult = await _d2RtuVeexLayer2.DeleteTest(rtuAddresses, $@"tests/{oldTest.id}");
                    if (delResult.HttpStatusCode != HttpStatusCode.NoContent)
                        return new BaseRefAssignedDto(){ ReturnCode = ReturnCode.BaseRefAssignmentFailed };
                }

                BaseRefAssignedDto createResult = new BaseRefAssignedDto();
                foreach (var baseRefDto in dto.BaseRefDtos)
                {
                    createResult =
                        await _d2RtuVeexLayer2.FullTestCreation(rtuAddresses, dto.OtdrId,
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
