using System.Net;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Newtonsoft.Json;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public partial class D2RtuVeexLayer1
    {
        public async Task<HttpRequestResult> SetBaseRef(DoubleAddress rtuDoubleAddress, string testId, byte[] sorBytes, byte[] sorBytes2 = null)
        {
            var res = await _httpExt.PostByteArray(
                rtuDoubleAddress, $@"monitoring/{testId}/references", sorBytes, sorBytes2);
            res.IsSuccessful = res.HttpStatusCode == HttpStatusCode.Created;
            return res;
        }

        public async Task<HttpRequestResult> GetTestThresholds(DoubleAddress rtuDoubleAddress, string testLink)
        {
            var res = await _httpExt.RequestByUrl(rtuDoubleAddress, $@"monitoring/{testLink}/thresholds/current", "get");
            res.IsSuccessful = res.HttpStatusCode == HttpStatusCode.OK;
            if (res.IsSuccessful) res.ResponseObject = JsonConvert.DeserializeObject<ThresholdSet>(res.ResponseJson);
            return res;
        }

        public async Task<HttpRequestResult> SetThresholds(DoubleAddress rtuDoubleAddress, string testLink,
            ThresholdSet thresholdSet)
        {
            var jsonData = JsonConvert.SerializeObject(thresholdSet);
            var res = await _httpExt.RequestByUrl(
                rtuDoubleAddress, $@"monitoring/{testLink}/thresholds", "post", "application/json", jsonData);
            res.IsSuccessful = res.HttpStatusCode == HttpStatusCode.Created;
            return res;
        }
    }
}
