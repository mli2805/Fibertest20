using System;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class FakeWcfServiceForClient : IWcfServiceForClient
    {
        public string SendCommand(string json)
        {
            return null;
        }

        public string[] GetEvents(int revision)
        {
            return new string[0];
        }

        public Task<ClientRegisteredDto> MakeExperimentAsync(RegisterClientDto dto)
        {
            throw new NotImplementedException();
        }

        public Task<ClientRegisteredDto> RegisterClientAsync(RegisterClientDto dto)
        {
            throw new NotImplementedException();
        }

        public void UnRegisterClient(UnRegisterClientDto dto)
        {
            throw new NotImplementedException();
        }

        public bool CheckServerConnection(CheckServerConnectionDto dto)
        {
            throw new NotImplementedException();
        }

        public bool CheckRtuConnection(CheckRtuConnectionDto rtuAddress)
        {
            throw new NotImplementedException();
        }

        public void InitializeRtuLongTask(InitializeRtuDto dto)
        {
            throw new NotImplementedException();
        }

        public bool InitializeRtu(InitializeRtuDto rtu)
        {
            throw new NotImplementedException();
        }

        public bool StartMonitoring(StartMonitoringDto dto)
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