using System.Collections.Generic;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public partial class D2RtuVeexLayer2
    {
        public async Task<Test> GetOrCreateTest(DoubleAddress rtuDoubleAddress, string otdrId, List<VeexOtauPort> otauPorts, BaseRefType baseRefType)
        {
            var test = await GetTestForPortAndBaseType(rtuDoubleAddress, otauPorts, baseRefType.ToString().ToLower());
            if (test != null)
                return test;

            var createResult = await CreateTest(rtuDoubleAddress, otdrId, otauPorts, baseRefType);
            return !createResult.IsSuccessful 
                ? null 
                : await GetTestForPortAndBaseType(rtuDoubleAddress, otauPorts, baseRefType.ToString().ToLower());
        }
        
        public async Task<BaseRefAssignedDto> SetBaseWithThresholdsForTest(DoubleAddress rtuDoubleAddress, string testLink,
            BaseRefDto dto, BaseRefDto dto2 = null)
        {
            var setBaseResult = await _d2RtuVeexLayer1.SetBaseRef(rtuDoubleAddress, testLink, dto.SorBytes, dto2?.SorBytes);
            if (!setBaseResult.IsSuccessful)
                return new BaseRefAssignedDto()
                { ReturnCode = ReturnCode.BaseRefAssignmentFailed, ErrorMessage = setBaseResult.ErrorMessage };

            var thresholds = dto.SorBytes.ExtractThresholds();
            if (thresholds == null)
                return new BaseRefAssignedDto() { ReturnCode = ReturnCode.BaseRefAssignmentNoThresholds };

            var setThresholdResult =
                await _d2RtuVeexLayer1.SetThresholds(rtuDoubleAddress, testLink, thresholds);

            return setThresholdResult.IsSuccessful
                ? new BaseRefAssignedDto() {ReturnCode = ReturnCode.BaseRefAssignedSuccessfully}
                : new BaseRefAssignedDto()
                    {ReturnCode = ReturnCode.BaseRefAssignmentFailed, ErrorMessage = setBaseResult.ErrorMessage};
        }
    }
}
