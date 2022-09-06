using System.Net;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Newtonsoft.Json;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public partial class D2RtuVeexLayer1
    {
        private static readonly JsonSerializerSettings IgnoreNulls =
            new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };
        private readonly HttpWrapper _httpWrapper;

        public D2RtuVeexLayer1(HttpWrapper httpWrapper)
        {
            _httpWrapper = httpWrapper;
        }

        public async Task<HttpRequestResult> GetMonitoringProperties(DoubleAddress rtuDoubleAddress)
        {
            var res = await _httpWrapper.RequestByUrl(rtuDoubleAddress, "monitoring", "get");
            res.IsSuccessful = res.HttpStatusCode == HttpStatusCode.OK;
            if (res.IsSuccessful)
                res.ResponseObject = JsonConvert.DeserializeObject<VeexMonitoringDto>(res.ResponseJson);
            return res;
        }

        // only one property could be changed at a time
        public async Task<HttpRequestResult> SetMonitoringProperty(DoubleAddress rtuDoubleAddress, string propertyName, string propertyValue)
        {
            var json = $"{{\"{propertyName}\":\"{propertyValue}\"}}";
            var res = await _httpWrapper.RequestByUrl(rtuDoubleAddress,
                "monitoring", "patch", "application/merge-patch+json", json);
            res.IsSuccessful = res.HttpStatusCode == HttpStatusCode.NoContent;
            return res;
        }

        public async Task<HttpRequestResult> ChangeProxyMode(DoubleAddress rtuDoubleAddress, string otdrId, bool isProxyEnabled)
        {
            var word = isProxyEnabled ? "true" : "false";
            var json = $"{{\"enabled\":{word}}}";
            var res = await _httpWrapper.RequestByUrl(rtuDoubleAddress,
                $"otdrs/{otdrId}/tcp_proxy", "patch", "application/merge-patch+json", json);
            res.IsSuccessful = res.HttpStatusCode == HttpStatusCode.NoContent;
            return res;
        }

        public async Task<HttpRequestResult> DisableVesionIntegration(DoubleAddress rtuDoubleAddress)
        {
            var json = $"{{\"state\":\"disabled\"}}";
            var res = await _httpWrapper.RequestByUrl(rtuDoubleAddress,
                $"vesion/settings", "patch", "application/merge-patch+json", json);
            res.IsSuccessful = res.HttpStatusCode == HttpStatusCode.NoContent;
            return res;
        }

        public async Task<HttpRequestResult> SwitchOtauToPort(DoubleAddress rtuDoubleAddress, string otauId, int port)
        {
            var jsonData = $"{{\"portIndex\":{port}}}";
            var res = await _httpWrapper.RequestByUrl(rtuDoubleAddress,
                $@"otaus/{otauId}", "patch", "application/merge-patch+json", jsonData);
            res.IsSuccessful = res.HttpStatusCode == HttpStatusCode.NoContent;
            return res;
        }

        public async Task<HttpRequestResult> DoMeasurementRequest(DoubleAddress rtuDoubleAddress, VeexMeasurementRequest dto)
        {
            var jsonData = JsonConvert.SerializeObject(dto, IgnoreNulls);
            var res = await _httpWrapper.RequestByUrl(rtuDoubleAddress,
                @"measurements", "post", "application/json", jsonData);
            res.IsSuccessful = res.HttpStatusCode == HttpStatusCode.Created;
            return res;
        }

        public async Task<HttpRequestResult> GetMeasurementResult(DoubleAddress rtuDoubleAddress, string measId)
        {
            var res = await _httpWrapper.RequestByUrl(rtuDoubleAddress, $"measurements/{measId}", "get");
            res.IsSuccessful = res.HttpStatusCode == HttpStatusCode.OK;
            res.ResponseObject = JsonConvert.DeserializeObject<VeexMeasurementResult>(res.ResponseJson);
            return res;
        }

        public async Task<HttpRequestResult> GetMeasurementBytes(DoubleAddress rtuDoubleAddress, string measId)
        {
            var res = await _httpWrapper.GetByteArray(rtuDoubleAddress, $"measurements/{measId}/traces/0");
            res.IsSuccessful = res.HttpStatusCode == HttpStatusCode.OK;
            return res;
        }

        public async Task<HttpRequestResult> StartOutOfTurnPreciseMeasurement(DoubleAddress rtuDoubleAddress, string testId)
        {
            var jsonData = "{}";
            var res = await _httpWrapper.RequestByUrl(rtuDoubleAddress,
                $@"monitoring/tests/{testId}/out_of_turn", "post", "application/json", jsonData);
            res.IsSuccessful = res.HttpStatusCode == HttpStatusCode.NoContent;
            return res;
        }
    }
}
