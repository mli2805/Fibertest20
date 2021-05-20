using System.Net;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public class D2RtuVeexLayer21
    {
        private readonly D2RtuVeexLayer2 _d2RtuVeexLayer2;

        public D2RtuVeexLayer21(D2RtuVeexLayer2 d2RtuVeexLayer2)
        {
            _d2RtuVeexLayer2 = d2RtuVeexLayer2;
        }

        public async Task<BaseRefAssignedDto> FullTestCreation(DoubleAddress rtuDoubleAddress, string otdrId, int portIndex, BaseRefDto dto)
        {
            var createResult = await _d2RtuVeexLayer2.CreateTest(rtuDoubleAddress, otdrId, portIndex, dto);
            if (createResult.HttpStatusCode != HttpStatusCode.Created)
                return new BaseRefAssignedDto()
                { ReturnCode = ReturnCode.BaseRefAssignmentFailed, ErrorMessage = createResult.ErrorMessage };
            var testLink = createResult.ResponseJson;

            return await _d2RtuVeexLayer2.SetBaseWithThresholdsForTest(rtuDoubleAddress, testLink, dto);
        }

        public async Task<BaseRefAssignedDto> ReSetBaseRefs(DoubleAddress rtuDoubleAddress, string otdrId, int portIndex, BaseRefDto dto)
        {
            var test = await _d2RtuVeexLayer2.GetTestForPortAndBaseType(rtuDoubleAddress, portIndex, dto.BaseRefType.ToString());
            if (test != null)
                return await _d2RtuVeexLayer2.SetBaseWithThresholdsForTest(rtuDoubleAddress, test.id, dto);
            else 
                return await FullTestCreation(rtuDoubleAddress, otdrId, portIndex, dto);
        }
    }
}
