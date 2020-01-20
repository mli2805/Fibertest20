using System;
using System.Collections.Generic;
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
                var rtuInitializedDto = await GetSettings(rtuAddresses, dto);
                SaveServerUrl(rtuAddresses, dto);

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

        private async Task<RtuInitializedDto> GetSettings(DoubleAddress rtuDoubleAddress, InitializeRtuDto dto)
        {
            var result = await _d2RtuVeexLayer2.GetSettings(rtuDoubleAddress, dto);
            if (!result.IsInitialized)
                throw new Exception(result.ErrorMessage);
            return result;
        }

        private async void SaveServerUrl(DoubleAddress rtuDoubleAddress, InitializeRtuDto dto)
        {
            var serverNotificationSettings = new ServerNotificationSettings()
            {
                state = "enabled",
                eventTypes = new List<string>() { "monitoring_test_failed", "monitoring_test_passed" },
                url = $@"http://{dto.ServerAddresses.Main.ToStringASpace}/",
            };
            var result = await _d2RtuVeexLayer2.SetServerUrl(rtuDoubleAddress, serverNotificationSettings);
            if (result.HttpStatusCode != HttpStatusCode.NoContent)
                throw new Exception(result.ErrorMessage);
        }


    }
}
