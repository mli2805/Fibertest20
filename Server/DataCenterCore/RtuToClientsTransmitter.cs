using System;
using System.Threading.Tasks;
using Iit.Fibertest.DatabaseLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.DataCenterCore
{
    public class RtuToClientsTransmitter
    {
        private readonly IMyLog _logFile;
        private readonly ClientRegistrationManager _clientRegistrationManager;
        private readonly D2CWcfManager _d2CWcfManager;

        public RtuToClientsTransmitter(IMyLog logFile,
            ClientRegistrationManager clientRegistrationManager, D2CWcfManager d2CWcfManager)
        {
            _logFile = logFile;
            _clientRegistrationManager = clientRegistrationManager;
            _d2CWcfManager = d2CWcfManager;
        }

        public bool NotifyUsersRtuCurrentMonitoringStep(CurrentMonitoringStepDto dto)
        {
            try
            {
                var addresses = _clientRegistrationManager.GetClientsAddresses().Result;
                if (addresses == null)
                    return true;
                _d2CWcfManager.SetClientsAddresses(addresses);
            }
            catch (Exception e)
            {
                _logFile.AppendLine("Get or Set ClientsAddresses: " + e.Message);
                return false;
            }
            try
            {
                return _d2CWcfManager.NotifyUsersRtuCurrentMonitoringStep(dto).Result == 0;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("RtuToClientsTransmitter.NotifyUsersRtuCurrentMonitoringStep: " + e.Message);
                return false;
            }
        }
    }
}
