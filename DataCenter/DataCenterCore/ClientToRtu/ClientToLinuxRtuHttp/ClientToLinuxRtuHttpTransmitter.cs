using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterCore
{
    public class ClientToLinuxRtuHttpTransmitter : IClientToRtuTransmitter
    {
        private readonly IMyLog _logFile;
        private readonly ClientsCollection _clientsCollection;
        private readonly IMakLinuxConnector _makLinuxConnector;

        public ClientToLinuxRtuHttpTransmitter(IMyLog logFile, 
            ClientsCollection clientsCollection, IMakLinuxConnector makLinuxConnector)
        {
            _logFile = logFile;
            _clientsCollection = clientsCollection;
            _makLinuxConnector = makLinuxConnector;
        }

        public Task<RtuConnectionCheckedDto> CheckRtuConnection(CheckRtuConnectionDto dto)
        {
            _logFile.AppendLine($"Client {_clientsCollection.Get(dto.ConnectionId)} checks RTU {dto.NetAddress.ToStringA()} connection");
            return _makLinuxConnector.CheckRtuConnection(dto);
        }

        public Task<RtuInitializedDto> InitializeRtuAsync(InitializeRtuDto dto)
        {
            _logFile.AppendLine($"Client {_clientsCollection.Get(dto.ConnectionId)} initializes RTU {dto.RtuAddresses.Main.ToStringA()}");
            return _makLinuxConnector.InitializeRtu(dto); // could return InProgress or RtuIsBusy
        }

        public Task<RtuCurrentStateDto> GetRtuCurrentState(GetCurrentRtuStateDto dto)
        {
            return _makLinuxConnector.GetRtuCurrentState(dto);
        }

        public Task<RequestAnswer> ApplyMonitoringSettingsAsync(ApplyMonitoringSettingsDto dto, DoubleAddress rtuDoubleAddress)
        {
            return _makLinuxConnector.ApplyMonitoringSettingsAsync(dto, rtuDoubleAddress);
        }

        public Task<RequestAnswer> StopMonitoringAsync(StopMonitoringDto dto, DoubleAddress rtuDoubleAddress)
        {
            return _makLinuxConnector.StopMonitoringAsync(dto, rtuDoubleAddress);
        }

        public Task<BaseRefAssignedDto> TransmitBaseRefsToRtuAsync(AssignBaseRefsDto dto, DoubleAddress rtuDoubleAddress)
        {
            return _makLinuxConnector.TransmitBaseRefsToRtuAsync(dto, rtuDoubleAddress);
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