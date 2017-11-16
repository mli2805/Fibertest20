using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Graph.Tests
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

        public Task<OpticalEventsList> GetOpticalEvents(int revision)
        {
            throw new NotImplementedException();
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



        public Task<bool> StopMonitoringAsync(StopMonitoringDto dto)
        {
            throw new NotImplementedException();
        }

        public Task<MonitoringSettingsAppliedDto> ApplyMonitoringSettingsAsync(ApplyMonitoringSettingsDto settings)
        {
            throw new NotImplementedException();
        }

        public Task<BaseRefAssignedDto> AssignBaseRefAsync(AssignBaseRefDto baseRef)
        {
            throw new NotImplementedException();
        }

    }
}