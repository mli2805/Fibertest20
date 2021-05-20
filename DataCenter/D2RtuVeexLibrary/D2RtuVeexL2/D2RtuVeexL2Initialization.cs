using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public partial class D2RtuVeexLayer2
    {
        public async Task<RtuInitializedDto> GetSettings(DoubleAddress rtuDoubleAddress, InitializeRtuDto dto)
        {
            var result = new RtuInitializedDto();

            var platformResponse = await _d2RtuVeexLayer1.GetPlatformSettings(rtuDoubleAddress, result);
            if (!platformResponse)
                return new RtuInitializedDto { ReturnCode = ReturnCode.RtuInitializationError };

            var otdrResponse = await _d2RtuVeexLayer1.GetOtdrSettings(rtuDoubleAddress, result);
            if (!otdrResponse)
                return new RtuInitializedDto { ReturnCode = ReturnCode.RtuInitializationError };

            var otauResponse = await _d2RtuVeexLayer1.GetOtauSettings(rtuDoubleAddress, result);
            if (!otauResponse)
                return new RtuInitializedDto { ReturnCode = ReturnCode.OtauInitializationError };

            result.RtuId = dto.RtuId;
            result.RtuAddresses = dto.RtuAddresses;
            result.OtdrAddress = (NetAddress)result.RtuAddresses.Main.Clone();
            result.Maker = RtuMaker.VeEX;
            result.ReturnCode = ReturnCode.RtuInitializedSuccessfully;

            var monitoringResponse = await _d2RtuVeexLayer1.GetMonitoringMode(rtuDoubleAddress, result);
            if (!monitoringResponse)
                return new RtuInitializedDto { ReturnCode = ReturnCode.RtuInitializationError };

            result.IsInitialized = true;
            return result;
        }

        public async Task<HttpRequestResult> SetServerNotificationUrl(DoubleAddress rtuDoubleAddress, InitializeRtuDto dto)
        {
            var serverNotificationSettings = new ServerNotificationSettings()
            {
                state = "enabled",
                eventTypes = new List<string>()
                {
                    "monitoring_test_failed", 
                    "monitoring_test_passed"
                },
                url = $@"http://{dto.ServerAddresses.Main.ToStringA()}/veex/notify?rtuId={dto.RtuId}",
            };

            return await _d2RtuVeexLayer1.SetServerNotificationUrl(rtuDoubleAddress, serverNotificationSettings);
        }

        // monitoring mode could not be changed if otdr in "proxy" mode (for reflect connection)
        // if it is so - proxy mode should be changed
        public async Task<bool> SetMonitoringMode(DoubleAddress rtuAddresses, string otdrId, string mode)
        {
            _logFile.AppendLine("SetMonitoringMode:");
            var httpRequestResult = await _d2RtuVeexLayer1.SetMonitoringMode(rtuAddresses, mode);
            _logFile.AppendLine($"SetMonitoringMode request result status code: { httpRequestResult.HttpStatusCode}");
            if (httpRequestResult.HttpStatusCode == HttpStatusCode.Conflict)
            {
                var proxy = await _d2RtuVeexLayer1.ChangeProxyMode(rtuAddresses, otdrId);
                _logFile.AppendLine($"ChangeProxyMode request result status code: { proxy.HttpStatusCode}");
                if (proxy.HttpStatusCode != HttpStatusCode.NoContent)
                    return false;

                httpRequestResult = await _d2RtuVeexLayer1.SetMonitoringMode(rtuAddresses, mode);
            }

            return httpRequestResult.HttpStatusCode == HttpStatusCode.NoContent;
        }

    }
}
