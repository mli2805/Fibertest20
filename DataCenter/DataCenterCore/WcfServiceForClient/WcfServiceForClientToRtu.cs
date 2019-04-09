using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.DataCenterCore
{
    public partial class WcfServiceForClient
    {
        public async Task<RtuConnectionCheckedDto> CheckRtuConnectionAsync(CheckRtuConnectionDto dto)
        {
            return await _clientToRtuTransmitter.CheckRtuConnection(dto);
        }

        public async Task<RtuInitializedDto> InitializeRtuAsync(InitializeRtuDto dto)
        {
            return await _clientToRtuTransmitter.InitializeAsync(dto);
        }

        public async Task<OtauAttachedDto> AttachOtauAsync(AttachOtauDto dto)
        {
            return await _clientToRtuTransmitter.AttachOtauAsync(dto);
        }

        public async Task<OtauDetachedDto> DetachOtauAsync(DetachOtauDto dto)
        {
            return await _clientToRtuTransmitter.DetachOtauAsync(dto);
        }

        public async Task<bool> StopMonitoringAsync(StopMonitoringDto dto)
        {
            return await _clientToRtuTransmitter.StopMonitoringAsync(dto);
        }

        public async Task<MonitoringSettingsAppliedDto> ApplyMonitoringSettingsAsync(ApplyMonitoringSettingsDto dto)
        {
            return await _clientToRtuTransmitter.ApplyMonitoringSettingsAsync(dto);
        }

        public async Task<BaseRefAssignedDto> AssignBaseRefAsync(AssignBaseRefsDto dto)
        {
               return await _clientToRtuTransmitter.AssignBaseRefAsync(dto);
        }

        // Base refs had been assigned earlier (and saved in Db) and now user attached trace to the port
        // base refs should be extracted from Db and sent to the RTU
        // or user explicitly demands to resend base refs to RTU 
        public async Task<BaseRefAssignedDto> ReSendBaseRefAsync(ReSendBaseRefsDto dto)
        {
              return await _clientToRtuTransmitter.ReSendBaseRefAsync(dto);
        }
      
        public async Task<ClientMeasurementStartedDto> DoClientMeasurementAsync(DoClientMeasurementDto dto)
        {
            return await _clientToRtuTransmitter.DoClientMeasurementAsync(dto);
        }

        public async Task<OutOfTurnMeasurementStartedDto> DoOutOfTurnPreciseMeasurementAsync(DoOutOfTurnPreciseMeasurementDto dto)
        {
            return await _clientToRtuTransmitter.DoOutOfTurnPreciseMeasurementAsync(dto);
        }
    }
}