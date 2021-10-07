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
            var res = await _httpWrapper.RequestByUrl(rtuDoubleAddress,
                @"otaus", "post", "application/json", jsonData);
            res.IsSuccessful = res.HttpStatusCode == HttpStatusCode.Created;
            return res;
        }

        public async Task<HttpRequestResult> DeleteOtau(DoubleAddress rtuDoubleAddress, string otauId)
        {
            var res = await _httpWrapper.RequestByUrl(rtuDoubleAddress,
                $@"otaus/{otauId}", "delete", "application/json");
            res.IsSuccessful = res.HttpStatusCode == HttpStatusCode.NoContent;
            return res;
        }

        public async Task<HttpRequestResult> ChangeOtauCascadingScheme(DoubleAddress rtuDoubleAddress,
            VeexOtauCascadingScheme scheme)
        {
            var jsonData = JsonConvert.SerializeObject(scheme);
            var res = await _httpWrapper.RequestByUrl(rtuDoubleAddress,
                @"otau_cascading", "patch", "application/merge-patch+json", jsonData);
            res.IsSuccessful = res.HttpStatusCode == HttpStatusCode.NoContent;
            return res;
        }

        public async Task<HttpRequestResult> GetOtauCascadingScheme(DoubleAddress rtuDoubleAddress)
        {
            var res = await _httpWrapper.RequestByUrl(rtuDoubleAddress,
                @"otau_cascading", "get", "application/json");

            res.IsSuccessful = res.HttpStatusCode == HttpStatusCode.OK;
            if (res.IsSuccessful)
                res.ResponseObject = JsonConvert.DeserializeObject<VeexOtauCascadingScheme>(res.ResponseJson);

            if (res.ResponseObject == null)
            {
                res.ErrorMessage = "Failed to parse cascading scheme!";
                res.HttpStatusCode = HttpStatusCode.ExpectationFailed;
                return res;
            }

            return res;
        }

        public async Task<HttpRequestResult> GetOtau(DoubleAddress rtuDoubleAddress, string link)
        {
            var res = await _httpWrapper.RequestByUrl(rtuDoubleAddress,
                $@"{link}", "get", "application/json");

            res.IsSuccessful = res.HttpStatusCode == HttpStatusCode.OK;
            if (res.IsSuccessful)
                res.ResponseObject = JsonConvert.DeserializeObject<VeexOtau>(res.ResponseJson);

            return res;
        }

        public async Task<HttpRequestResult> GetOtaus(DoubleAddress rtuDoubleAddress)
        {
            var res = await _httpWrapper.RequestByUrl(rtuDoubleAddress, "otaus", "get");
            res.IsSuccessful = res.HttpStatusCode == HttpStatusCode.OK;
            if (res.IsSuccessful)
                res.ResponseObject = JsonConvert.DeserializeObject<LinkList>(res.ResponseJson);

            return res;
        }

    }
}
