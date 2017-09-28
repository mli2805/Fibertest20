using System;
using System.Threading.Tasks;
using Dto;
using WcfServiceForClientLibrary;

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
            throw new System.NotImplementedException();
        }

        public void RegisterClient(RegisterClientDto dto)
        {
            throw new System.NotImplementedException();
        }

        public void UnRegisterClient(UnRegisterClientDto dto)
        {
            throw new System.NotImplementedException();
        }

        public bool CheckServerConnection(CheckServerConnectionDto dto)
        {
            throw new System.NotImplementedException();
        }

        public bool CheckRtuConnection(CheckRtuConnectionDto rtuAddress)
        {
            throw new System.NotImplementedException();
        }

        public void InitializeRtuLongTask(InitializeRtuDto dto)
        {
            throw new NotImplementedException();
        }

        public bool InitializeRtu(InitializeRtuDto rtu)
        {
            throw new System.NotImplementedException();
        }

        public bool StartMonitoring(StartMonitoringDto dto)
        {
            throw new System.NotImplementedException();
        }

        public bool StopMonitoring(StopMonitoringDto dto)
        {
            throw new System.NotImplementedException();
        }

        public bool ApplyMonitoringSettings(ApplyMonitoringSettingsDto settings)
        {
            throw new System.NotImplementedException();
        }

        public bool AssignBaseRef(AssignBaseRefDto baseRef)
        {
            throw new System.NotImplementedException();
        }
    }
}