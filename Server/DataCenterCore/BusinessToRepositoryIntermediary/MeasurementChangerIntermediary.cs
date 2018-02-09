using System.Threading.Tasks;
using Iit.Fibertest.DatabaseLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.DataCenterCore
{
    public class MeasurementChangerIntermediary
    {
        private readonly MeasurementsRepository _measurementsRepository;
        private readonly ClientStationsRepository _clientStationsRepository;
        private readonly D2CWcfManager _d2CWcfManager;

        public MeasurementChangerIntermediary(MeasurementsRepository measurementsRepository, 
            ClientStationsRepository clientStationsRepository, D2CWcfManager d2CWcfManager)
        {
            _measurementsRepository = measurementsRepository;
            _clientStationsRepository = clientStationsRepository;
            _d2CWcfManager = d2CWcfManager;
        }

        public async Task<MeasurementUpdatedDto> UpdateMeasurementAsync(UpdateMeasurementDto dto)
        {
            var result = await _measurementsRepository.SaveMeasurementChangesAsync(dto);
            if (result.ReturnCode == ReturnCode.Ok)
            {
                await Task.Factory.StartNew(() => NotifyUsersMeasurementUpdated(result));
            }
            return result;
        }

        private async void NotifyUsersMeasurementUpdated(MeasurementUpdatedDto dto)
        {
            var addresses = await _clientStationsRepository.GetClientsAddresses();
            if (addresses == null)
                return;
            _d2CWcfManager.SetClientsAddresses(addresses);
            await _d2CWcfManager.NotifyUsersMeasurementUpdated(dto);
        }
    }
}