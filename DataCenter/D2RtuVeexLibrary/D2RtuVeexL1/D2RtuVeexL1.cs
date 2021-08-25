using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Newtonsoft.Json;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public partial class D2RtuVeexLayer1
    {
        private readonly HttpExt _httpExt;

        public D2RtuVeexLayer1(HttpExt httpExt)
        {
            _httpExt = httpExt;
        }

        public async Task<HttpRequestResult> SetMonitoringState(DoubleAddress rtuDoubleAddress, string state)
        {
            var json = JsonConvert.SerializeObject(new MonitoringVeexDto() { state = state });
            var httpResult = await _httpExt.RequestByUrl(rtuDoubleAddress,
                "monitoring", "patch", "application/merge-patch+json", json);
            return httpResult;
        }

        public async Task<HttpRequestResult> SetMonitoringTypeToFibertest(DoubleAddress rtuDoubleAddress)
        {
            var json = JsonConvert.SerializeObject(new MonitoringVeexDto() { type = "fibertest" });
            var httpResult = await _httpExt.RequestByUrl(rtuDoubleAddress,
                "monitoring", "patch", "application/merge-patch+json", json);
            return httpResult;
        }

        public async Task<HttpRequestResult> ChangeProxyMode(DoubleAddress rtuDoubleAddress, string otdrId)
        {
            var httpResult = await _httpExt.RequestByUrl(rtuDoubleAddress,
                $"otdrs/{otdrId}", "patch", "application/merge-patch+json", "");
            return httpResult;
        }

        public async Task<HttpRequestResult> SetServerNotificationSettings(DoubleAddress rtuDoubleAddress, ServerNotificationSettings dto)
        {
            var jsonData = JsonConvert.SerializeObject(dto);
            return await _httpExt.RequestByUrl(rtuDoubleAddress,
                $@"/notification/settings", "patch", "application/merge-patch+json", jsonData);
        }

        public async Task<HttpRequestResult> SetBaseRef(DoubleAddress rtuDoubleAddress, string testId, byte[] sorBytes)
        {
            return await _httpExt.PostByteArray(rtuDoubleAddress, $@"monitoring/{testId}/references", sorBytes);
        }
    }
}
