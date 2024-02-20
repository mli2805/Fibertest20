using Iit.Fibertest.Dto;
using System.Threading.Tasks;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.DataCenterCore
{
    public partial class WcfIntermediateC2R
    {
        public async Task<RequestAnswer> StopMonitoringAsync(StopMonitoringDto dto)
        {
            if (!TryToGetClientAndOccupyRtu(dto.ConnectionId, dto.RtuId, RtuOccupation.MonitoringSettings,
                    out RequestAnswer response))
                return response;
            var rtuAddresses = await _rtuStationsRepository.GetRtuAddresses(dto.RtuId);
            if (rtuAddresses == null)
                return new RequestAnswer(ReturnCode.NoSuchRtu);

            var result = await GetRtuSpecificTransmitter(rtuAddresses.Main.Port).StopMonitoringAsync(dto, rtuAddresses);
            if (result.ReturnCode == ReturnCode.Ok)
            {
                var cmd = new StopMonitoring { RtuId = dto.RtuId };
                await _eventStoreService.SendCommand(cmd, response.UserName, dto.ClientIp);
                await _ftSignalRClient.NotifyAll("MonitoringStopped", cmd.ToCamelCaseJson());
            }

            _rtuOccupations.TrySetOccupation(dto.RtuId, RtuOccupation.None, response.UserName, out RtuOccupationState _);

            return result;
        }

        public async Task<RequestAnswer> InterruptMeasurementAsync(InterruptMeasurementDto dto)
        {
            if (!TryToGetClient(dto.ConnectionId, dto.RtuId, RtuOccupation.None, out RequestAnswer response))
                return response;

            var rtuAddresses = await _rtuStationsRepository.GetRtuAddresses(dto.RtuId);
            if (rtuAddresses == null)
                return new RequestAnswer(ReturnCode.NoSuchRtu);

            return await GetRtuSpecificTransmitter(rtuAddresses.Main.Port).InterruptMeasurementAsync(dto, rtuAddresses);
        }

        public async Task<RequestAnswer> FreeOtdrAsync(FreeOtdrDto dto)
        {
            if (!TryToGetClient(dto.ConnectionId, dto.RtuId, RtuOccupation.None, out RequestAnswer response))
                return response;

            var rtuAddresses = await _rtuStationsRepository.GetRtuAddresses(dto.RtuId);
            if (rtuAddresses == null)
                return new RequestAnswer(ReturnCode.NoSuchRtu);

            return await GetRtuSpecificTransmitter(rtuAddresses.Main.Port).FreeOtdrAsync(dto, rtuAddresses);
        }
    }
}
