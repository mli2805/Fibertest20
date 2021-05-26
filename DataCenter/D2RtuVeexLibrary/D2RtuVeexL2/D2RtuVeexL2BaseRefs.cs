using System.Net;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public partial class D2RtuVeexLayer2
    {
        public async Task<BaseRefAssignedDto> SetBaseWithThresholdsForTest(DoubleAddress rtuDoubleAddress, string testLink,
            BaseRefDto dto)
        {
            var setBaseResult = await _d2RtuVeexLayer1.SetBaseRef(rtuDoubleAddress, testLink, dto.SorBytes);
            if (setBaseResult.HttpStatusCode != HttpStatusCode.Created)
                return new BaseRefAssignedDto()
                { ReturnCode = ReturnCode.BaseRefAssignmentFailed, ErrorMessage = setBaseResult.ErrorMessage };

            var thresholds = dto.SorBytes.ExtractThresholds();
            if (thresholds == null)
                return new BaseRefAssignedDto() { ReturnCode = ReturnCode.BaseRefAssignmentNoThresholds };

            var setThresholdResult =
                await _d2RtuVeexLayer1.SetThresholds(rtuDoubleAddress, testLink, thresholds);

            return setThresholdResult.HttpStatusCode != HttpStatusCode.Created
                ? new BaseRefAssignedDto()
                    { ReturnCode = ReturnCode.BaseRefAssignmentFailed, ErrorMessage = setBaseResult.ErrorMessage }
                : new BaseRefAssignedDto() { ReturnCode = ReturnCode.BaseRefAssignedSuccessfully };
        }
    }
}
