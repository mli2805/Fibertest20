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
            if (!createResult.IsSuccessful)
                return null;

            var getResult = await _d2RtuVeexLayer1.GetTest(rtuDoubleAddress, createResult.ResponseJson);
            return !getResult.IsSuccessful
                ? null
                : (Test)getResult.ResponseObject;
        }

        public async Task<BaseRefAssignedDto> SetBaseRefsForTest(DoubleAddress rtuDoubleAddress, string testLink,
            BaseRefDto dto, BaseRefDto dto2 = null)
        {
            var setBaseResult = await _d2RtuVeexLayer1.SetBaseRef(rtuDoubleAddress, testLink, dto.SorBytes, dto2?.SorBytes);
            return setBaseResult.IsSuccessful 
                ? new BaseRefAssignedDto { ReturnCode = ReturnCode.BaseRefAssignedSuccessfully }
                : new BaseRefAssignedDto
                    { ReturnCode = ReturnCode.BaseRefAssignmentFailed, ErrorMessage = setBaseResult.ErrorMessage };
        }

    }
}
