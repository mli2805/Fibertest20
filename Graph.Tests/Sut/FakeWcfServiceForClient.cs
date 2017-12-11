using System;
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

//        string[] IWcfServiceForClient.GetEvents(int revision)
//        {
//            return new string[0];
//        }

        public Task<MeasurementsList> GetOpticalEvents(int revision)
        {
            throw new NotImplementedException();
        }

        public Task<NetworkEventsList> GetNetworkEvents(int revision)
        {
            throw new NotImplementedException();
        }

        public Task<TraceStatistics> GetTraceStatistics(Guid traceId)
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> GetSorBytesOfBase(Guid baseRefId)
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> GetSorBytesOfMeasurement(int sorFileId)
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> GetSorBytesOfLastTraceMeasurement(Guid traceId)
        {
            throw new NotImplementedException();
        }

        public Task<Measurement> GetLastTraceMeasurement(Guid traceId)
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

        public Task<BaseRefAssignedDto> AssignBaseRefAsync(AssignBaseRefsDto baseRefs)
        {
            throw new NotImplementedException();
        }

        public Task<BaseRefAssignedDto> ReSendBaseRefAsync(ReSendBaseRefsDto baseRefs)
        {
            throw new NotImplementedException();
        }
    }
}