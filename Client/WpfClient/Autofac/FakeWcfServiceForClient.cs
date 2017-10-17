using System;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class FakeWcfServiceForClient : IWcfServiceForClient
    {
        public Task<string> SendCommandAsObj(object cmd)
        {
            return null;
        }

        Task<string> IWcfServiceForClient.SendCommand(string json)
        {
            return null;
        }

        Task<string[]> IWcfServiceForClient.GetEvents(int revision)
        {
            return Task.FromResult(new string[0]);
        }


        public Task<ClientRegisteredDto> MakeExperimentAsync(RegisterClientDto dto)
        {
            throw new NotImplementedException();
        }

        public Task<ClientRegisteredDto> RegisterClientAsync(RegisterClientDto dto)
        {
            throw new NotImplementedException();
        }

        public Task UnregisterClientAsync(UnRegisterClientDto dto)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CheckServerConnection(CheckServerConnectionDto dto)
        {
            throw new NotImplementedException();
        }

        public Task<RtuConnectionCheckedDto> CheckRtuConnectionAsync(CheckRtuConnectionDto dto)
        {
            throw new NotImplementedException();
        }

        public Task<RtuInitializedDto> InitializeRtuAsync(InitializeRtuDto dto)
        {
            throw new NotImplementedException();
        }

        public Task<bool> StartMonitoringAsync(StartMonitoringDto dto)
        {
            throw new NotImplementedException();
        }



        public bool StopMonitoring(StopMonitoringDto dto)
        {
            throw new NotImplementedException();
        }

        public bool ApplyMonitoringSettings(ApplyMonitoringSettingsDto settings)
        {
            throw new NotImplementedException();
        }

        public bool AssignBaseRef(AssignBaseRefDto baseRef)
        {
            throw new NotImplementedException();
        }

    }
}