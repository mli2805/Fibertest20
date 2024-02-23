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
            return _makLinuxConnector
                .SendCommand<InitializeRtuDto, RtuInitializedDto>(dto, dto.RtuAddresses); // could return InProgress or RtuIsBusy
        }

        public Task<RtuCurrentStateDto> GetRtuCurrentState(GetCurrentRtuStateDto dto)
        {
            return _makLinuxConnector.GetRtuCurrentState(dto);
        }

        public Task<RequestAnswer> ApplyMonitoringSettingsAsync(ApplyMonitoringSettingsDto dto, DoubleAddress rtuDoubleAddress)
        {
            return _makLinuxConnector
                .SendCommand<ApplyMonitoringSettingsDto, RequestAnswer>(dto, rtuDoubleAddress);
        }

        public Task<RequestAnswer> StopMonitoringAsync(StopMonitoringDto dto, DoubleAddress rtuDoubleAddress)
        {
            return _makLinuxConnector.SendCommand<StopMonitoringDto, RequestAnswer>(dto, rtuDoubleAddress);
        }

        // Full dto with base refs (sorBytes) is serialized into json here and de-serialized on RTU
        public Task<BaseRefAssignedDto> TransmitBaseRefsToRtuAsync(AssignBaseRefsDto dto, DoubleAddress rtuDoubleAddress)
        {
            return _makLinuxConnector.SendCommand<AssignBaseRefsDto, BaseRefAssignedDto>(dto, rtuDoubleAddress);
        }

        public Task<RequestAnswer> DoOutOfTurnPreciseMeasurementAsync(DoOutOfTurnPreciseMeasurementDto dto, DoubleAddress rtuDoubleAddress)
        {
            return _makLinuxConnector.SendCommand<DoOutOfTurnPreciseMeasurementDto, RequestAnswer>(dto, rtuDoubleAddress);
        }

        public Task<RequestAnswer> InterruptMeasurementAsync(InterruptMeasurementDto dto, DoubleAddress rtuDoubleAddress)
        {
            return _makLinuxConnector.SendCommand<InterruptMeasurementDto, RequestAnswer>(dto, rtuDoubleAddress);
        }

        public Task<RequestAnswer> FreeOtdrAsync(FreeOtdrDto dto, DoubleAddress rtuDoubleAddress)
        {
            return _makLinuxConnector.SendCommand<FreeOtdrDto, RequestAnswer>(dto, rtuDoubleAddress);
        }

        public Task<ClientMeasurementStartedDto> DoClientMeasurementAsync(DoClientMeasurementDto dto, DoubleAddress rtuDoubleAddress)
        {
            return _makLinuxConnector.SendCommand<DoClientMeasurementDto, ClientMeasurementStartedDto>(dto, rtuDoubleAddress);
        }

        public Task<ClientMeasurementVeexResultDto> GetMeasurementClientResultAsync(GetClientMeasurementDto dto)
        {
            // VeEx
            throw new System.NotImplementedException();
        }

        public Task<ClientMeasurementVeexResultDto> GetClientMeasurementSorBytesAsync(GetClientMeasurementDto dto)
        {
            // VeEx
            throw new System.NotImplementedException();
        }

        public Task<OtauAttachedDto> AttachOtauAsync(AttachOtauDto dto, DoubleAddress rtuDoubleAddress)
        {
            return _makLinuxConnector.SendCommand<AttachOtauDto, OtauAttachedDto>(dto, rtuDoubleAddress);
        }

        public Task<OtauDetachedDto> DetachOtauAsync(DetachOtauDto dto, DoubleAddress rtuDoubleAddress)
        {
            return _makLinuxConnector.SendCommand<DetachOtauDto, OtauDetachedDto>(dto, rtuDoubleAddress);
        }
    }
}