using System.Net;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Newtonsoft.Json;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public partial class D2RtuVeexLayer1
    {
        public async Task<HttpRequestResult> CreateOtau(DoubleAddress rtuDoubleAddress, NewOtau newOtau)
        {
            var jsonData = JsonConvert.SerializeObject(newOtau);
            return await _httpExt.RequestByUrl(rtuDoubleAddress,
                $@"/otaus", "post", "application/json", jsonData);
        }

        public async Task<HttpRequestResult> DeleteOtau(DoubleAddress rtuDoubleAddress, string otauId)
        {
            return await _httpExt.RequestByUrl(rtuDoubleAddress,
                $@"/otaus/{otauId}", "delete", "application/json");
        }

        public async Task<HttpRequestResult> GetOtauCascadingScheme(DoubleAddress rtuDoubleAddress)
        {
            return await _httpExt.RequestByUrl(rtuDoubleAddress,
                $@"/otau_cascading", "get", "application/json");
        }

        public async Task<HttpRequestResult> ChangeOtauCascadingScheme(DoubleAddress rtuDoubleAddress,
            VeexOtauCascadingScheme scheme)
        {
            var jsonData = JsonConvert.SerializeObject(scheme);
            return await _httpExt.RequestByUrl(rtuDoubleAddress,
                $@"/otau_cascading", "patch", "application/merge-patch+json", jsonData);
        }

        public async Task<HttpRequestResult> GetOtau(DoubleAddress rtuDoubleAddress, string otauId)
        {
            var res = await _httpExt.RequestByUrl(rtuDoubleAddress,
                $@"/otaus/{otauId}", "get", "application/json");

            if (res.HttpStatusCode == HttpStatusCode.OK)
                res.ResponseObject = JsonConvert.DeserializeObject<VeexOtau>(res.ResponseJson);

            return res;
        }

    }
}
