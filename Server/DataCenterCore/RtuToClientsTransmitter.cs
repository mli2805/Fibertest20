using System.Threading.Tasks;
using Iit.Fibertest.DatabaseLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.DataCenterCore
{
    public class RtuToClientsTransmitter
    {
        private readonly ClientRegistrationManager _clientRegistrationManager;
        private readonly D2CWcfManager _d2CWcfManager;

        public RtuToClientsTransmitter(ClientRegistrationManager clientRegistrationManager, D2CWcfManager d2CWcfManager)
        {
            _clientRegistrationManager = clientRegistrationManager;
            _d2CWcfManager = d2CWcfManager;
        }

        private async Task<int> NotifyUsersRtuCurrentMonitoringStep(KnowRtuCurrentMonitoringStepDto dto)
        {
            var addresses = await _clientRegistrationManager.GetClientsAddresses();
            _d2CWcfManager.SetClientsAddresses(addresses);
            return await _d2CWcfManager.NotifyUsersRtuCurrentMonitoringStep(dto);
        }
    }
}
