using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public partial class D2RtuVeexLayer2
    {
        public async Task<HttpRequestResult> GetPlatform(DoubleAddress rtuDoubleAddress)
        {
            return await _d2RtuVeexLayer1.GetPlatform(rtuDoubleAddress);
        }

        public async Task<HttpRequestResult> DisableProxyMode(DoubleAddress rtuDoubleAddress, string otdrId)
        {
            return await _d2RtuVeexLayer1.ChangeProxyMode(rtuDoubleAddress, otdrId, false);
        }

        public async Task<HttpRequestResult> DisableVesionIntegration(DoubleAddress rtuDoubleAddress)
        {
            return await _d2RtuVeexLayer1.DisableVesionIntegration(rtuDoubleAddress);
        }

        public async Task<HttpRequestResult> GetMonitoringProperties(DoubleAddress rtuDoubleAddress)
        {
            return await _d2RtuVeexLayer1.GetMonitoringProperties(rtuDoubleAddress);
        }

        public async Task<RtuInitializedDto> InitializeMonitoringProperties(
            DoubleAddress rtuDoubleAddress, VeexMonitoringDto monitoringProperties)
        {
            if (monitoringProperties.state == "enabled")
            {
                var setStateRes =
                    await _d2RtuVeexLayer1.SetMonitoringProperty(rtuDoubleAddress, "state", "disabled");
                if (!setStateRes.IsSuccessful)
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

            var setResult = await _d2RtuVeexLayer1.SetMonitoringProperty(rtuDoubleAddress, "type", "fibertest");
            if (!setResult.IsSuccessful)
            {
                return new RtuInitializedDto()
                {
                    ReturnCode = ReturnCode.RtuInitializationError,
                    ErrorMessage = setResult.ErrorMessage + System.Environment.NewLine + setResult.ResponseJson,
                };
            }

            return null;
        }

     
    }
}
