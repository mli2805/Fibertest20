using System.Net;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Newtonsoft.Json;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public partial class D2RtuVeexLayer1
    {
        public async Task<HttpRequestResult> CreateOtau(DoubleAddress rtuDoubleAddress, CreateOtau createOtau)
        {
            var jsonData = JsonConvert.SerializeObject(createOtau);
            return await _httpExt.RequestByUrl(rtuDoubleAddress,
                $@"/otaus", "post", "application/json", jsonData);
           }

        public async Task<HttpRequestResult> DeleteOtau(DoubleAddress rtuDoubleAddress, string otauId)
        {
            return await _httpExt.RequestByUrl(rtuDoubleAddress,
                $@"/otaus/{otauId}", "delete", "application/json");
        }

        public async Task<HttpRequestResult> GetOtauCascading(DoubleAddress rtuDoubleAddress)
        {
            return await _httpExt.RequestByUrl(rtuDoubleAddress,
                $@"/otau_cascading", "get", "application/json");
        }

        public async Task<HttpRequestResult> ChangeOtauCascading(DoubleAddress rtuDoubleAddress, VeexOtauCascadingScheme scheme)
        {
            var jsonData = JsonConvert.SerializeObject(scheme);
            return await _httpExt.RequestByUrl(rtuDoubleAddress,
                $@"/otau_cascading", "post", "application/json", jsonData);
        }

    }


}
