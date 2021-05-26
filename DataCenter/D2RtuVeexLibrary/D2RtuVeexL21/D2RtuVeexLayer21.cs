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

        /// <summary>
        /// Creates test if does not exist
        /// </summary>
        /// <param name="rtuDoubleAddress"></param>
        /// <param name="otdrId"></param>
        /// <param name="otauId"></param>
        /// <param name="portIndex"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<BaseRefAssignedDto> ReSetBaseRefs(DoubleAddress rtuDoubleAddress, string otdrId, string otauId, int portIndex, BaseRefDto dto)
        {
            var test = await _d2RtuVeexLayer2.GetTestForPortAndBaseType(rtuDoubleAddress, portIndex, dto.BaseRefType.ToString().ToLower());
            string testLink;
            if (test == null)
            {
                var createResult = await _d2RtuVeexLayer2.CreateTest(rtuDoubleAddress, otdrId, otauId, portIndex, dto);
                if (createResult.HttpStatusCode != HttpStatusCode.Created)
                    return new BaseRefAssignedDto()
                        {ReturnCode = ReturnCode.BaseRefAssignmentFailed, ErrorMessage = createResult.ErrorMessage};
                testLink = createResult.ResponseJson;
            }
            else testLink = $@"tests/{test.id}";

            return await _d2RtuVeexLayer2.SetBaseWithThresholdsForTest(rtuDoubleAddress, testLink, dto);
        }

        public async Task<BaseRefAssignedDto> FullTestCreation(DoubleAddress rtuDoubleAddress, string otdrId, string otauId, int portIndex, BaseRefDto dto)
        {
            var createResult = await _d2RtuVeexLayer2.CreateTest(rtuDoubleAddress, otdrId, otauId, portIndex, dto);
            if (createResult.HttpStatusCode != HttpStatusCode.Created)
                return new BaseRefAssignedDto()
                { ReturnCode = ReturnCode.BaseRefAssignmentFailed, ErrorMessage = createResult.ErrorMessage };
            var testLink = createResult.ResponseJson;

            return await _d2RtuVeexLayer2.SetBaseWithThresholdsForTest(rtuDoubleAddress, testLink, dto);
        }


    }
}
