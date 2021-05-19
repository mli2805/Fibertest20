using System.Net;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public class D2RtuVeexLayer21
    {
        private readonly D2RtuVeexLayer1 _d2RtuVeexLayer1;
        private readonly D2RtuVeexLayer2 _d2RtuVeexLayer2;

        public D2RtuVeexLayer21(D2RtuVeexLayer1 d2RtuVeexLayer1, D2RtuVeexLayer2 d2RtuVeexLayer2)
        {
            _d2RtuVeexLayer1 = d2RtuVeexLayer1;
            _d2RtuVeexLayer2 = d2RtuVeexLayer2;
        }

        public async Task<BaseRefAssignedDto> FullTestCreation(DoubleAddress rtuDoubleAddress, string otdrId, int portIndex, BaseRefDto dto)
        {
            var thresholds = dto.SorBytes.ExtractThresholds();
            if (thresholds == null) return new BaseRefAssignedDto() { ReturnCode = ReturnCode.BaseRefAssignmentNoThresholds };

            var createResult = await _d2RtuVeexLayer2.CreateTest(rtuDoubleAddress, otdrId, portIndex, dto);
            if (createResult.HttpStatusCode != HttpStatusCode.Created)
                return new BaseRefAssignedDto()
                { ReturnCode = ReturnCode.BaseRefAssignmentFailed, ErrorMessage = createResult.ErrorMessage };
            var testLink = createResult.ResponseJson;

            var setBaseResult = await _d2RtuVeexLayer1.SetBaseRef(rtuDoubleAddress, $@"monitoring/{testLink}/references", dto.SorBytes);
            if (setBaseResult.HttpStatusCode != HttpStatusCode.Created)
                return new BaseRefAssignedDto()
                { ReturnCode = ReturnCode.BaseRefAssignmentFailed, ErrorMessage = setBaseResult.ErrorMessage };

            var setThresholdResult =
                await _d2RtuVeexLayer1.SetThresholds(rtuDoubleAddress, $@"monitoring/{testLink}/thresholds",
                    thresholds);
            if (setThresholdResult.HttpStatusCode != HttpStatusCode.OK)
                return new BaseRefAssignedDto()
                { ReturnCode = ReturnCode.BaseRefAssignmentFailed, ErrorMessage = setBaseResult.ErrorMessage };

            return new BaseRefAssignedDto() { ReturnCode = ReturnCode.BaseRefAssignedSuccessfully };
        }
    }
}
