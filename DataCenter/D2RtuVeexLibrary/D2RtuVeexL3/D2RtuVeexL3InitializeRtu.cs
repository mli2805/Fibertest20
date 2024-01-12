using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public partial class D2RtuVeexLayer3
    {
        public async Task<RtuInitializedDto> InitializeRtuAsync(InitializeRtuDto dto)
        {
            try
            {
                var rtuAddresses = (DoubleAddress)dto.RtuAddresses.Clone();
                var host = rtuAddresses.Main.GetVeexRtuUriHost();
                if (!_veexRtuAuthorizationDict.Dict.TryGetValue(host, out var rtuData))
                    _veexRtuAuthorizationDict.Dict.Add(host, new VeexRtuAuthorizationData()
                    {
                        RtuId = dto.RtuId,
                        Serial = dto.Serial,
                    });
                else rtuData.Serial = dto.Serial;

                var platformRes = await _d2RtuVeexLayer2.GetPlatform(rtuAddresses);
                if (!platformRes.IsSuccessful)
                    return new RtuInitializedDto
                    {
                        ReturnCode = platformRes.HttpStatusCode == HttpStatusCode.Unauthorized
                            ? ReturnCode.RtuUnauthorizedAccess
                            : ReturnCode.RtuInitializationError,
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
                if (!otdr.isConnected)
                {
                    var proxy = await _d2RtuVeexLayer2.DisableProxyMode(rtuAddresses, otdr.id);
                    if (!proxy.IsSuccessful)
                        return new RtuInitializedDto
                        {
                            ReturnCode = ReturnCode.RtuInitializationError,
                            ErrorMessage = "Failed to disable proxy mode!" + Environment.NewLine + proxy.ErrorMessage,
                        };
                }

                var otauRes = await _d2RtuVeexLayer2.InitializeOtaus(rtuAddresses, dto);
                if (!otauRes.IsSuccessful)
                    return new RtuInitializedDto
                    {
                        ReturnCode = ReturnCode.OtauInitializationError,
                        ErrorMessage = otauRes.ErrorMessage,
                    };

                var result = new RtuInitializedDto()
                   .FillInRtuSettings(dto)
                   .FillInPlatform((VeexPlatformInfo)platformRes.ResponseObject)
                   .FillInOtdr(otdr)
                   .FillInOtau((List<VeexOtau>)otauRes.ResponseObject, dto);
                _veexRtuAuthorizationDict.Dict[host].Serial = result.Serial; // in case user input with misprinting

                var res = await Configure(rtuAddresses, dto);
                if (res != null)
                {
                    result.ReturnCode = ReturnCode.RtuInitializationError;
                    result.ErrorMessage = res;
                    return result;
                }

                result.ReturnCode = ReturnCode.RtuInitializedSuccessfully;
                result.IsMonitoringOn = false;
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

        private async Task<string> Configure(DoubleAddress rtuAddresses, InitializeRtuDto dto)
        {
            var authRes = await _d2RtuVeexLayer2.EnableAuthorization(rtuAddresses, true);
            if (!authRes.IsSuccessful)
                return "Failed to set authorization mode";

            var getMoniPropResult = await _d2RtuVeexLayer2.GetMonitoringProperties(rtuAddresses);
            if (!getMoniPropResult.IsSuccessful)
                return "Failed to get monitoring properties";
            var monitoringProperties = (VeexMonitoringDto)getMoniPropResult.ResponseObject;

            if (monitoringProperties.state == "enabled")
            {
                var stateRes = await _d2RtuVeexLayer2.StopMonitoring(rtuAddresses);
                if (!stateRes.IsSuccessful)
                    return "Failed to stop monitoring";
            }

            if (dto.IsSynchronizationRequired || dto.IsFirstInitialization || monitoringProperties.type != "fibertest")
            {
                if (!await _d2RtuVeexLayer2.DeleteAllTests(rtuAddresses))
                    return "Failed to delete existing tests";
            }

            if (monitoringProperties.type != "fibertest")
            {
                var ftRes = await _d2RtuVeexLayer2.SetTypeAsFibertest(rtuAddresses);
                if (!ftRes.IsSuccessful)
                    return "Failed to set Fibertest mode";
            }

            return null;
        }
    }
}
