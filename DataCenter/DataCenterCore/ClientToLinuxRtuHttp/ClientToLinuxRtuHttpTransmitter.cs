using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterCore
{
    public class ClientToLinuxRtuHttpTransmitter : IClientToRtuTransmitter
    {
        private readonly IMyLog _logFile;
        private readonly ClientsCollection _clientsCollection;

        public ClientToLinuxRtuHttpTransmitter(IMyLog logFile, ClientsCollection clientsCollection)
        {
            _logFile = logFile;
            _clientsCollection = clientsCollection;
        }

        public Task<RtuConnectionCheckedDto> CheckRtuConnection(CheckRtuConnectionDto dto)
        {
            _logFile.AppendLine($"Client {_clientsCollection.Get(dto.ConnectionId)} check RTU {dto.NetAddress.ToStringA()} connection");
            throw new System.NotImplementedException();

        }

        public Task<RtuInitializedDto> InitializeRtuAsync(InitializeRtuDto dto)
        {
            throw new System.NotImplementedException();
        }

        public Task<MonitoringSettingsAppliedDto> ApplyMonitoringSettingsAsync(ApplyMonitoringSettingsDto dto)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> StopMonitoringAsync(StopMonitoringDto dto)
        {
            throw new System.NotImplementedException();
        }

        public Task<BaseRefAssignedDto> TransmitBaseRefsToRtuAsync(AssignBaseRefsDto dto)
        {
            throw new System.NotImplementedException();
        }

        public Task<RequestAnswer> DoOutOfTurnPreciseMeasurementAsync(DoOutOfTurnPreciseMeasurementDto dto)
        {
            throw new System.NotImplementedException();
        }

        public Task<RequestAnswer> InterruptMeasurementAsync(InterruptMeasurementDto dto)
        {
            throw new System.NotImplementedException();
        }

        public Task<RequestAnswer> FreeOtdrAsync(FreeOtdrDto dto)
        {
            throw new System.NotImplementedException();
        }

        public Task<ClientMeasurementStartedDto> DoClientMeasurementAsync(DoClientMeasurementDto dto)
        {
            throw new System.NotImplementedException();
        }

        public Task<ClientMeasurementVeexResultDto> GetMeasurementClientResultAsync(GetClientMeasurementDto dto)
        {
            throw new System.NotImplementedException();
        }

        public Task<ClientMeasurementVeexResultDto> GetClientMeasurementSorBytesAsync(GetClientMeasurementDto dto)
        {
            throw new System.NotImplementedException();
        }
    }
}