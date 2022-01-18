using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public partial class D2RtuVeexLayer2
    {
        public async Task<RtuInitializedDto> GetPlatformInfo(DoubleAddress rtuDoubleAddress, InitializeRtuDto dto)
        {
            var result = new RtuInitializedDto();

            var platformResponse = await _d2RtuVeexLayer1.GetPlatform(rtuDoubleAddress);
            if (!platformResponse.IsSuccessful)
                return new RtuInitializedDto { ReturnCode = ReturnCode.RtuInitializationError };
            FillInPlatform((VeexPlatformInfo)platformResponse.ResponseObject, result);

            result.RtuId = dto.RtuId;
            result.RtuAddresses = dto.RtuAddresses;
            result.OtdrAddress = (NetAddress)result.RtuAddresses.Main.Clone();
            result.Maker = RtuMaker.VeEX;
            result.ReturnCode = ReturnCode.RtuInitializedSuccessfully;

            result.IsInitialized = true;
            return result;
        }

        public async Task<HttpRequestResult> DisableProxyMode(DoubleAddress rtuDoubleAddress, string otdrId)
        {
            return await _d2RtuVeexLayer1.ChangeProxyMode(rtuDoubleAddress, otdrId, false);
        }

        public async Task<HttpRequestResult> DisableVesionIntegration(DoubleAddress rtuDoubleAddress)
        {
            return await _d2RtuVeexLayer1.DisableVesionIntegration(rtuDoubleAddress);
        }

        private void FillInPlatform(VeexPlatformInfo info, RtuInitializedDto result)
        {
            result.Mfid = info.platform.name;
            result.Mfsn = info.platform.serialNumber;
            result.Serial = info.platform.serialNumber;
            result.Version = info.components.api;
            result.Version2 = info.components.otdrEngine.iit_otdr;
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
