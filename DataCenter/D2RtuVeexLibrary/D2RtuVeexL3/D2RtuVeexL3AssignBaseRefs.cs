using System;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public partial class D2RtuVeexLayer3
    {
        public async Task<BaseRefAssignedDto> AssignBaseRefAsync(AssignBaseRefsDto dto, DoubleAddress rtuAddresses)
        {
            try
            {
                var createResult = new BaseRefAssignedDto();
                foreach (var baseRefDto in dto.BaseRefs)
                {
                    if (baseRefDto.Id == Guid.Empty) // it is command to delete such a base ref
                    {
                        var deleteResult = await _d2RtuVeexLayer2.DeleteTestForPortAndBaseType(rtuAddresses, dto.OtauPortDto.OpticalPort, baseRefDto.BaseRefType.ToString().ToLower());
                        if (!deleteResult)
                            return new BaseRefAssignedDto()
                            {
                                ReturnCode = ReturnCode.BaseRefAssignmentFailed,
                                ErrorMessage = "Failed to delete old base ref",
                            };
                    }
                    else
                    {
                        var testLink = await _d2RtuVeexLayer2.GetOrCreateTest(rtuAddresses, dto.OtdrId, dto.OtauPortDto.OtauId, dto.OtauPortDto.OpticalPort, baseRefDto);
                        if (testLink == null)
                            return new BaseRefAssignedDto() { ReturnCode = ReturnCode.BaseRefAssignmentFailed };

                        createResult =
                            await _d2RtuVeexLayer2.SetBaseWithThresholdsForTest(rtuAddresses, testLink, baseRefDto);
                        if (createResult.ReturnCode != ReturnCode.BaseRefAssignedSuccessfully)
                            return createResult;
                    }
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
