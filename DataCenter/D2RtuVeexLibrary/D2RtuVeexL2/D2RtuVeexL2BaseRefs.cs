using System.Net;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public partial class D2RtuVeexLayer2
    {
        public async Task<BaseRefAssignedDto> SetBaseWithThresholdsForTest(DoubleAddress rtuDoubleAddress, string testId,
            BaseRefDto dto)
        {
            var setBaseResult = await _d2RtuVeexLayer1.SetBaseRef(rtuDoubleAddress, $@"monitoring/tests/{testId}/references", dto.SorBytes);
            if (setBaseResult.HttpStatusCode != HttpStatusCode.Created)
                return new BaseRefAssignedDto()
                { ReturnCode = ReturnCode.BaseRefAssignmentFailed, ErrorMessage = setBaseResult.ErrorMessage };

            var thresholds = dto.SorBytes.ExtractThresholds();
            if (thresholds == null)
                return new BaseRefAssignedDto() { ReturnCode = ReturnCode.BaseRefAssignmentNoThresholds };

            var setThresholdResult =
                await _d2RtuVeexLayer1.SetThresholds(rtuDoubleAddress, $@"monitoring/tests/{testId}/thresholds",
                    thresholds);
            if (setThresholdResult.HttpStatusCode != HttpStatusCode.OK)
                return new BaseRefAssignedDto()
                { ReturnCode = ReturnCode.BaseRefAssignmentFailed, ErrorMessage = setBaseResult.ErrorMessage };

            return new BaseRefAssignedDto() { ReturnCode = ReturnCode.BaseRefAssignedSuccessfully };
        }
    }
}
