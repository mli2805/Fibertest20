using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Newtonsoft.Json;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public partial class D2RtuVeexLayer1
    {
        private static readonly JsonSerializerSettings IgnoreNulls =
            new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };
        private readonly HttpExt _httpExt;

        public D2RtuVeexLayer1(HttpExt httpExt)
        {
            _httpExt = httpExt;
        }

        public async Task<HttpRequestResult> SetMonitoringState(DoubleAddress rtuDoubleAddress, string state)
        {
            return await SetMonitoringProperty(rtuDoubleAddress, "state", state);
        }

        public async Task<HttpRequestResult> SetMonitoringTypeToFibertest(DoubleAddress rtuDoubleAddress)
        {
            return await SetMonitoringProperty(rtuDoubleAddress, "type", "fibertest");
        }

        // only one property could be changed at a time
        private async Task<HttpRequestResult> SetMonitoringProperty(DoubleAddress rtuDoubleAddress, string propertyName, string propertyValue)
        {
            var json = $"{{\"{propertyName}\":\"{propertyValue}\"}}";
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

        public async Task<HttpRequestResult> SwitchOtauToPort(DoubleAddress rtuDoubleAddress, string otauId, int port)
        {
            var jsonData = $"{{\"portIndex\":\"{port}\"}}";
            return await _httpExt.RequestByUrl(rtuDoubleAddress,
                $@"/otaus/{otauId}", "patch", "application/merge-patch+json", jsonData);
        }

        public async Task<HttpRequestResult> DoMeasurementRequest(DoubleAddress rtuDoubleAddress, VeexMeasurementRequest dto)
        {
            var jsonData = JsonConvert.SerializeObject(dto);
            return await _httpExt.RequestByUrl(rtuDoubleAddress,
                $@"/measurements", "post", "application/json", jsonData);
        }


    }
}
