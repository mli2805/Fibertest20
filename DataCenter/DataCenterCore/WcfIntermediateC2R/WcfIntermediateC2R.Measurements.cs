using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.DataCenterCore
{
    public partial class WcfIntermediateC2R
    {
        public async Task<ClientMeasurementStartedDto> DoClientMeasurementAsync(DoClientMeasurementDto dto)
        {
            if (!TryToGetClientAndOccupyRtu(dto.ConnectionId, dto.RtuId, RtuOccupation.MeasurementClient,
                    out ClientMeasurementStartedDto response))
                return response;
            var rtuAddresses = await _rtuStationsRepository.GetRtuAddresses(dto.RtuId);
            if (rtuAddresses == null)
                return new ClientMeasurementStartedDto(ReturnCode.NoSuchRtu);

            return await GetRtuSpecificTransmitter(rtuAddresses.Main.Port).DoClientMeasurementAsync(dto, rtuAddresses);
        }

        public async Task<RequestAnswer> DoOutOfTurnPreciseMeasurementAsync(DoOutOfTurnPreciseMeasurementDto dto)
        {
            if (!TryToGetClientAndOccupyRtu(dto.ConnectionId, dto.RtuId, RtuOccupation.PreciseMeasurementOutOfTurn, out RequestAnswer response))
                return response;
            var rtuAddresses = await _rtuStationsRepository.GetRtuAddresses(dto.RtuId);
            if (rtuAddresses == null)
                return new RequestAnswer(ReturnCode.NoSuchRtu);

            return await GetRtuSpecificTransmitter(rtuAddresses.Main.Port)
                .DoOutOfTurnPreciseMeasurementAsync(dto, rtuAddresses);
        }
    }
}
