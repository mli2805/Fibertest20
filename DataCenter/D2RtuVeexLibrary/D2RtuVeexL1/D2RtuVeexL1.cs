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

        public async Task<HttpRequestResult> SetMonitoringMode(DoubleAddress rtuDoubleAddress, string mode)
        {
            var json = JsonConvert.SerializeObject(new MonitoringVeexDto() { state = mode });
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

        public async Task<HttpRequestResult> SetServerNotificationUrl(DoubleAddress rtuDoubleAddress, ServerNotificationSettings dto)
        {
            var jsonData = JsonConvert.SerializeObject(dto);
            return await _httpExt.RequestByUrl(rtuDoubleAddress,
                $@"/notification/settings", "patch", "application/merge-patch+json", jsonData);
        }

        public async Task<HttpRequestResult> SetBaseRef(DoubleAddress rtuDoubleAddress, string refUri, byte[] sorBytes)
        {
            return await _httpExt.PostFile(rtuDoubleAddress, refUri, sorBytes);
        }



    }
}
