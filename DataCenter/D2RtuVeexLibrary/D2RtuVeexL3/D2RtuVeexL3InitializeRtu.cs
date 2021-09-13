using System;
using System.Net;
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
                var rtuInitializedDto = await _d2RtuVeexLayer2.GetSettings(rtuAddresses, dto);
                if (!rtuInitializedDto.IsInitialized)
                    return rtuInitializedDto;

                if (dto.IsFirstInitialization)
                {
                    var schemeRes = await _d2RtuVeexLayer2.InitializeCascadingScheme(rtuAddresses, rtuInitializedDto.OtauId);
                    if (schemeRes.HttpStatusCode != HttpStatusCode.NoContent)
                    {
                        rtuInitializedDto.OwnPortCount = 0; // cos scheme initialization is made only while OwnPortCount is 0
                        rtuInitializedDto.ReturnCode = ReturnCode.RtuInitializationError;
                        rtuInitializedDto.ErrorMessage = "Failed to set main OTAU as a root in cascading scheme!" + Environment.NewLine + schemeRes.ErrorMessage;
                        return rtuInitializedDto;
                    }
                }

                var initRes = await _d2RtuVeexLayer2.InitializeMonitoringProperties(rtuAddresses);
                if (initRes != null)
                {
                    rtuInitializedDto.ErrorMessage = initRes.ErrorMessage;
                    rtuInitializedDto.ReturnCode = initRes.ReturnCode;
                    return rtuInitializedDto;
                }

                var saveRes = await _d2RtuVeexLayer2.SetServerNotificationSettings(rtuAddresses, dto);
                if (saveRes.HttpStatusCode != HttpStatusCode.NoContent)
                    return new RtuInitializedDto()
                    {
                        ReturnCode = ReturnCode.RtuInitializationError,
                        ErrorMessage = saveRes.ErrorMessage,
                    };

                rtuInitializedDto.ReturnCode = ReturnCode.RtuInitializedSuccessfully;
                return rtuInitializedDto;
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
