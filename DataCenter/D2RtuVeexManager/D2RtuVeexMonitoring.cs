using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HttpLib;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Newtonsoft.Json;

namespace D2RtuVeexManager
{
    public class D2RtuVeexMonitoring
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly IMyLog _logFile;
        private DoubleAddress _rtuDoubleAddress;

        public D2RtuVeexMonitoring(IMyLog logFile)
        {
            _logFile = logFile;
        }

        public async Task<MonitoringStoppedDto> Monitoring(DoubleAddress rtuDoubleAddress, string cmd, string prm)
        {
            _httpClient.DefaultRequestHeaders.ExpectContinue = false;

            _rtuDoubleAddress = (DoubleAddress)rtuDoubleAddress.Clone();

            var result = new MonitoringStoppedDto();
            var uri = $"http://{_rtuDoubleAddress.Main.ToStringA()}/api/v1/{cmd}";

            var json = JsonConvert.SerializeObject(new MonitoringVeexDto() { state = prm });

            var request = new HttpRequestMessage(new HttpMethod("PATCH"), uri);
            request.Content = new StringContent(json, Encoding.UTF8, "application/merge-patch+json");

            try
            {
                var responseMessage = await _httpClient.SendAsync(request);
                if (responseMessage == null)
                {
                    result.ReturnCode = ReturnCode.RtuMonitoringSettingsApplyError;
                    result.ErrorMessage = "RTU do not responded";
                    return result;
                }
                if (responseMessage.StatusCode != HttpStatusCode.OK)
                {
                    result.ReturnCode = ReturnCode.RtuMonitoringSettingsApplyError;
                    result.ErrorMessage = responseMessage.ReasonPhrase;
                    return result;
                }

                result.ReturnCode = ReturnCode.MonitoringSettingsAppliedSuccessfully;
                return result;
            }
            catch (Exception e)
            {
                result.ReturnCode = ReturnCode.RtuMonitoringSettingsApplyError;
                result.ErrorMessage = e.Message;
                _logFile.AppendLine("Monitoring: " + e.Message);
                return result;
            }
        }

    }
}