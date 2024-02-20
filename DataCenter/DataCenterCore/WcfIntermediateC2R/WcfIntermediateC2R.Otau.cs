using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.DataCenterCore
{
    public partial class WcfIntermediateC2R
    {
        public async Task<OtauAttachedDto> AttachOtauAsync(AttachOtauDto dto)
        {
            if (!TryToGetClientAndOccupyRtu(dto.ConnectionId, dto.RtuId, RtuOccupation.AttachOrDetachOtau, out OtauAttachedDto response))
                return response;
            var rtuAddresses = await _rtuStationsRepository.GetRtuAddresses(dto.RtuId);
            if (rtuAddresses == null) return new OtauAttachedDto(ReturnCode.NoSuchRtu);

            var result = await GetRtuSpecificTransmitter(rtuAddresses.Main.Port).AttachOtauAsync(dto, rtuAddresses);

            if (result.IsAttached)
            {
                AttachOtauIntoGraph(dto, result);
                await _ftSignalRClient.NotifyAll("FetchTree", null);
            }

            _rtuOccupations.TrySetOccupation(dto.RtuId, RtuOccupation.None, response.UserName, out RtuOccupationState _);

            return result;
        }

        private async void AttachOtauIntoGraph(AttachOtauDto dto, OtauAttachedDto result)
        {
            var username = _clientsCollection.Get(dto.ConnectionId)?.UserName;
            var cmd = new AttachOtau
            {
                Id = result.OtauId,
                RtuId = dto.RtuId,
                MasterPort = dto.OpticalPort,
                Serial = result.Serial,
                PortCount = result.PortCount,
                NetAddress = dto.NetAddress.Clone(),
                IsOk = true,
            };
            await _eventStoreService.SendCommand(cmd, username, dto.ClientIp);
        }

        public async Task<OtauDetachedDto> DetachOtauAsync(DetachOtauDto dto)
        {
            if (!TryToGetClientAndOccupyRtu(dto.ConnectionId, dto.RtuId, RtuOccupation.AttachOrDetachOtau, out OtauDetachedDto response))
                return response;
            var rtuAddresses = await _rtuStationsRepository.GetRtuAddresses(dto.RtuId);
            if (rtuAddresses == null) return new OtauDetachedDto(ReturnCode.NoSuchRtu);

            var result = await GetRtuSpecificTransmitter(rtuAddresses.Main.Port).DetachOtauAsync(dto, rtuAddresses);

            if (result.IsDetached)
            {
                var res = await RemoveOtauFromGraph(dto);
                if (string.IsNullOrEmpty(res))
                    await _ftSignalRClient.NotifyAll("FetchTree", null);
            }

            _rtuOccupations.TrySetOccupation(dto.RtuId, RtuOccupation.None, response.UserName, out RtuOccupationState _);

            return result;
        }

        private async Task<string> RemoveOtauFromGraph(DetachOtauDto dto)
        {
            var username = _clientsCollection.Get(dto.ConnectionId)?.UserName;
            var otau = _writeModel.Otaus.FirstOrDefault(o => o.Id == dto.OtauId);
            if (otau == null) return null;
            var cmd = new DetachOtau
            {
                Id = dto.OtauId,
                RtuId = dto.RtuId,
                OtauIp = dto.NetAddress.Ip4Address,
                TcpPort = dto.NetAddress.Port,
                TracesOnOtau = _writeModel.Traces
                    .Where(t => t.OtauPort != null && t.OtauPort.Serial == otau.Serial)
                    .Select(t => t.TraceId)
                    .ToList(),
            };

            return await _eventStoreService.SendCommand(cmd, username, dto.ClientIp);
        }
    }
}
