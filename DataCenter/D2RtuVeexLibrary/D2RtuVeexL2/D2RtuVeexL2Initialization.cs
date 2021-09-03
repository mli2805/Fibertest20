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

          
            result.IsInitialized = true;
            return result;
        }

        public async Task<RtuInitializedDto> InitializeMonitoringProperties(DoubleAddress rtuDoubleAddress)
        {
            var monitoringProperties = await _d2RtuVeexLayer1.GetMonitoringProperties(rtuDoubleAddress);
            if (monitoringProperties == null)
                return new RtuInitializedDto()
                {
                    ReturnCode = ReturnCode.RtuInitializationError, 
                    ErrorMessage = "Failed to get monitoring properties"
                };

            if (monitoringProperties.type == "fibertest") return null;

            if (monitoringProperties.state == "enabled")
            {
                var setStateRes = await _d2RtuVeexLayer1.SetMonitoringState(rtuDoubleAddress, "disabled");
                if (setStateRes.HttpStatusCode != HttpStatusCode.NoContent)
                    return new RtuInitializedDto()
                    {
                        ReturnCode = ReturnCode.RtuInitializationError, 
                        ErrorMessage = setStateRes.ErrorMessage,
                    };
            }

            if (!await DeleteAllTests(rtuDoubleAddress))
            {
                return new RtuInitializedDto()
                {
                    ReturnCode = ReturnCode.RtuInitializationError, 
                    ErrorMessage = "Failed to delete existing tests",
                };
            }

            var setResult = await _d2RtuVeexLayer1.SetMonitoringTypeToFibertest(rtuDoubleAddress);
            if (setResult.HttpStatusCode != HttpStatusCode.NoContent)
            {
                return new RtuInitializedDto()
                {
                    ReturnCode = ReturnCode.RtuInitializationError, 
                    ErrorMessage = setResult.ErrorMessage + System.Environment.NewLine + setResult.ResponseJson,
                };
            }
            return null;
        }

        public async Task<HttpRequestResult> SetServerNotificationSettings(DoubleAddress rtuDoubleAddress, InitializeRtuDto dto)
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

            return await _d2RtuVeexLayer1.SetServerNotificationSettings(rtuDoubleAddress, serverNotificationSettings);
        }
    }
}
