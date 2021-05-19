using System.Net;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Newtonsoft.Json;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public partial class D2RtuVeexLayer1
    {
        public async Task<ThresholdSet> GetTestThresholds(DoubleAddress rtuDoubleAddress, string setUri)
        {
            var httpResult = await _httpExt.RequestByUrl(rtuDoubleAddress, setUri, "get");
            return httpResult.HttpStatusCode == HttpStatusCode.OK
                ? JsonConvert.DeserializeObject<ThresholdSet>(httpResult.ResponseJson)
                : null;
        }

        public async Task<HttpRequestResult> SetThresholds(DoubleAddress rtuDoubleAddress, string thresholdUri,
            ThresholdSet thresholdSet)
        {
            var jsonData = JsonConvert.SerializeObject(thresholdSet);
            return await _httpExt.RequestByUrl(
                rtuDoubleAddress, thresholdUri, "post", "application/json", jsonData);
        }

    }
}
