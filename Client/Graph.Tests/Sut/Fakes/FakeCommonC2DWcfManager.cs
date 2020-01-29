using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.WcfConnections;

namespace Graph.Tests
{
    public class FakeCommonC2DWcfManager : IWcfServiceCommonC2D
    {
        public void SetServerAddresses(DoubleAddress newServerAddress, string username, string clientIp)
        {
            throw new System.NotImplementedException();
        }

        public Task<ClientRegisteredDto> RegisterClientAsync(RegisterClientDto dto)
        {
            throw new System.NotImplementedException();
        }

        public Task<RtuConnectionCheckedDto> CheckRtuConnectionAsync(CheckRtuConnectionDto rtuAddress)
        {
            throw new System.NotImplementedException();
        }

        public Task<RtuInitializedDto> InitializeRtuAsync(InitializeRtuDto dto)
        {
            throw new System.NotImplementedException();
        }

        public Task<OtauAttachedDto> AttachOtauAsync(AttachOtauDto dto)
        {
            throw new System.NotImplementedException();
        }

        public Task<OtauDetachedDto> DetachOtauAsync(DetachOtauDto dto)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> StopMonitoringAsync(StopMonitoringDto dto)
        {
            throw new System.NotImplementedException();
        }

        public Task<MonitoringSettingsAppliedDto> ApplyMonitoringSettingsAsync(ApplyMonitoringSettingsDto settings)
        {
            throw new System.NotImplementedException();
        }

        public Task<BaseRefAssignedDto> AssignBaseRefAsync(AssignBaseRefsDto baseRefs)
        {
            throw new System.NotImplementedException();
        }

        public Task<BaseRefAssignedDto> AssignBaseRefAsyncFromMigrator(AssignBaseRefsDto baseRefs)
        {
            throw new System.NotImplementedException();
        }

        public Task<BaseRefAssignedDto> ReSendBaseRefAsync(ReSendBaseRefsDto baseRefs)
        {
            throw new System.NotImplementedException();
        }

        public Task<ClientMeasurementStartedDto> DoClientMeasurementAsync(DoClientMeasurementDto dto)
        {
            throw new System.NotImplementedException();
        }

        public Task<OutOfTurnMeasurementStartedDto> DoOutOfTurnPreciseMeasurementAsync(DoOutOfTurnPreciseMeasurementDto dto)
        {
            throw new System.NotImplementedException();
        }
    }
}