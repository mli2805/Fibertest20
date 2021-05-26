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

        public async Task<string> GetOrCreateTest(DoubleAddress rtuDoubleAddress, string otdrId, string otauId, int portIndex, BaseRefDto dto)
        {
            var test = await _d2RtuVeexLayer2.GetTestForPortAndBaseType(rtuDoubleAddress, portIndex, dto.BaseRefType.ToString().ToLower());
            if (test != null)
                return $@"tests/{test.id}";

            var createResult = await _d2RtuVeexLayer2.CreateTest(rtuDoubleAddress, otdrId, otauId, portIndex, dto);
            return createResult.HttpStatusCode != HttpStatusCode.Created ? null : createResult.ResponseJson;
        }
    }
}
