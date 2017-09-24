using Dto;

namespace Graph.Tests
{
    public class FakeWcfServiceForClient : WcfServiceForClientLibrary.IWcfServiceForClient
    {
        public string SendCommand(string json)
        {
            throw new System.NotImplementedException();
        }

        public string[] GetEvents(int revision)
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
