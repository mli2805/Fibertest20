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

                var ftTypeRes = await _d2RtuVeexLayer2.SetMonitoringTypeToFibertest(rtuAddresses);
                if (ftTypeRes.HttpStatusCode != HttpStatusCode.NoContent)
                    return new RtuInitializedDto()
                    {
                        ReturnCode = ReturnCode.RtuInitializationError,
                        ErrorMessage = ftTypeRes.ErrorMessage,
                    };  

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
