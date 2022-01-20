using System;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public partial class D2RtuVeexLayer3
    {
        public async Task<RtuInitializedDto> InitializeRtu(InitializeRtuDto dto)
        {
            try
            {
                var rtuAddresses = (DoubleAddress)dto.RtuAddresses.Clone();

                var platformRes = await _d2RtuVeexLayer2.GetPlatform(rtuAddresses);
                if (!platformRes.IsSuccessful)
                    return new RtuInitializedDto
                    {
                        ReturnCode = ReturnCode.RtuInitializationError,
                        ErrorMessage = platformRes.ErrorMessage,
                    };

                var vesionRes = await _d2RtuVeexLayer2.DisableVesionIntegration(rtuAddresses);
                if (!vesionRes.IsSuccessful)
                    return new RtuInitializedDto()
                    {
                        ReturnCode = ReturnCode.RtuInitializationError,
                        ErrorMessage = vesionRes.ErrorMessage
                    };

                var otdrRes = await _d2RtuVeexLayer2.InitializeOtdrs(rtuAddresses);
                if (!otdrRes.IsSuccessful)
                    return new RtuInitializedDto()
                    {
                        ReturnCode = ReturnCode.OtdrInitializationFailed, 
                        ErrorMessage = otdrRes.ErrorMessage
                    };
                var otdr = (VeexOtdr)otdrRes.ResponseObject;

                var otauRes = await _d2RtuVeexLayer2.InitializeOtaus(rtuAddresses, dto);
                if (!otauRes.IsSuccessful)
                    return new RtuInitializedDto
                    {
                        ReturnCode = ReturnCode.OtauInitializationError,
                        ErrorMessage = otauRes.ErrorMessage,
                    };

                var proxy = await _d2RtuVeexLayer2.DisableProxyMode(rtuAddresses, otdr.id);
                if (!proxy.IsSuccessful)
                    return new RtuInitializedDto
                    {
                        ReturnCode = ReturnCode.RtuInitializationError,
                        ErrorMessage = "Failed to disable proxy mode!" + Environment.NewLine + proxy.ErrorMessage,
                    };

                var getMoniPropResult = await _d2RtuVeexLayer2.GetMonitoringProperties(rtuAddresses);
                if (!getMoniPropResult.IsSuccessful)
                    return new RtuInitializedDto()
                    {
                        ReturnCode = ReturnCode.RtuInitializationError,
                        ErrorMessage = "Failed to get monitoring properties"
                    };

                var result = new RtuInitializedDto()
                    .FillInRtuSettings(dto)
                    .FillInPlatform((VeexPlatformInfo)platformRes.ResponseObject)
                    .FillInOtdr(otdr)
                    .FillInOtau((VeexOtauInfo)otauRes.ResponseObject, dto);

                var monitoringProperties = (VeexMonitoringDto)getMoniPropResult.ResponseObject;
                if (monitoringProperties.type != "fibertest" || dto.IsFirstInitialization)
                {
                    var initRes = await _d2RtuVeexLayer2.InitializeMonitoringProperties(rtuAddresses, monitoringProperties);
                    if (initRes != null)
                    {
                        result.ErrorMessage = initRes.ErrorMessage;
                        result.ReturnCode = initRes.ReturnCode;
                        return result;
                    }

                    monitoringProperties.state = "disabled";
                }

                result.ReturnCode = ReturnCode.RtuInitializedSuccessfully;
                result.IsMonitoringOn = monitoringProperties.state == "enabled";
                return result;
            }
            catch (Exception e)
            {
                return new RtuInitializedDto()
                {
                    ReturnCode = ReturnCode.RtuInitializationError,
                    ErrorMessage = e.Message,
                };
            }
        }
      
    }
}
