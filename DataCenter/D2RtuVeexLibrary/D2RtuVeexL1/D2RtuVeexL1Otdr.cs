using System.Net;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Newtonsoft.Json;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public partial class D2RtuVeexLayer1
    {
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

        public async Task<HttpRequestResult> ReinitializeOtdr(DoubleAddress rtuDoubleAddress, string otdrId)
        {
            // empty json "{}" - reinitialize all otdrs, but if /otdrs contains some links, they should be reinitialized separately 
            var content = $"{{ \"otdrId\": {otdrId}}}"; 
            var res = await _httpWrapper
                .RequestByUrl(rtuDoubleAddress, "otdr_reconnection_requests", "post", content);
            res.IsSuccessful = res.HttpStatusCode == HttpStatusCode.Created;
            return res;
        }

        public async Task<HttpRequestResult> GetReinitializeOtdrStatus(DoubleAddress rtuDoubleAddress, string link)
        {
            var res = await _httpWrapper
                .RequestByUrl(rtuDoubleAddress, $"{link}", "get");
            res.IsSuccessful = res.HttpStatusCode == HttpStatusCode.OK;
            if (res.IsSuccessful)
                res.ResponseObject = JsonConvert.DeserializeObject<ReinitializeOtdrStatus>(res.ResponseJson);
            return res;
        }

    }
}
