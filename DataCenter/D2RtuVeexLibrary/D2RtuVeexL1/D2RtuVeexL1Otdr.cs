using System.Net;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Newtonsoft.Json;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public partial class D2RtuVeexLayer1
    {
        public async Task<HttpRequestResult> EnableAuthorization(DoubleAddress rtuDoubleAddress, bool param)
        {
            var json = $"{{\"apiAuthEnabled\":{param.ToString().ToLower()}}}"; 
            var res = await _httpWrapper
                .RequestByUrl(rtuDoubleAddress, "info", "patch", "application/merge-patch+json", json);
            res.IsSuccessful = res.HttpStatusCode == HttpStatusCode.NoContent;
            return res;
        }

        public async Task<HttpRequestResult> GetPlatform(DoubleAddress rtuDoubleAddress)
        {
            var res = await _httpWrapper.RequestByUrl(rtuDoubleAddress, "info", "get");
            res.IsSuccessful = res.HttpStatusCode == HttpStatusCode.OK;
            if (res.IsSuccessful)
                res.ResponseObject = JsonConvert.DeserializeObject<VeexPlatformInfo>(res.ResponseJson);
            return res;
        }

        public async Task<HttpRequestResult> GetOtdrs(DoubleAddress rtuDoubleAddress)
        {
            var res = await _httpWrapper.RequestByUrl(rtuDoubleAddress, "otdrs", "get");
            res.IsSuccessful = res.HttpStatusCode == HttpStatusCode.OK;
            if (res.IsSuccessful)
                res.ResponseObject = JsonConvert.DeserializeObject<LinkList>(res.ResponseJson);
            return res;
        }

        public async Task<HttpRequestResult> GetOtdr(DoubleAddress rtuDoubleAddress, string link)
        {
            var res = await _httpWrapper.RequestByUrl(rtuDoubleAddress, $"{link}", "get");
            res.IsSuccessful = res.HttpStatusCode == HttpStatusCode.OK;
            if (res.IsSuccessful)
                res.ResponseObject = JsonConvert.DeserializeObject<VeexOtdr>(res.ResponseJson);
            return res;
        }

        public async Task<HttpRequestResult> ResetOtdr(DoubleAddress rtuDoubleAddress, string otdrId)
        {
            // empty json "{}" - reset all otdrs, but if /otdrs contains some links, they should be reinitialized separately 
            var content = otdrId == "" ? "{}" : $"{{ \"otdrId\": \"{otdrId}\"}}"; 
            var res = await _httpWrapper
                .RequestByUrl(rtuDoubleAddress, "otdr_reconnection_requests", "post", "application/json", content);
            res.IsSuccessful = res.HttpStatusCode == HttpStatusCode.Created;
            return res;
        }

        public async Task<HttpRequestResult> GetResetOtdrStatus(DoubleAddress rtuDoubleAddress, string link)
        {
            var res = await _httpWrapper
                .RequestByUrl(rtuDoubleAddress, $"{link}", "get");
            res.IsSuccessful = res.HttpStatusCode == HttpStatusCode.OK;
            if (res.IsSuccessful)
                res.ResponseObject = JsonConvert.DeserializeObject<OtdrResetResponse>(res.ResponseJson);
            return res;
        }

    }
}
