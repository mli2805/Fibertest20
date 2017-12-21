using System;
using System.ServiceModel;
using Iit.Fibertest.DatabaseLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WcfServiceForRtuInterface;

namespace Iit.Fibertest.DataCenterCore
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class WcfServiceForRtu : IWcfServiceForRtu
    {
        private readonly IMyLog _logFile;
        private readonly ClientStationsRepository _clientStationsRepository;
        private readonly RtuStationsRepository _rtuStationsRepository;
        private readonly D2CWcfManager _d2CWcfManager;

        public WcfServiceForRtu(IMyLog logFile,
            ClientStationsRepository clientStationsRepository,
            RtuStationsRepository rtuStationsRepository,
            D2CWcfManager d2CWcfManager)
        {
            _logFile = logFile;
            _clientStationsRepository = clientStationsRepository;
            _rtuStationsRepository = rtuStationsRepository;
            _d2CWcfManager = d2CWcfManager;
        }

        public void NotifyUserCurrentMonitoringStep(CurrentMonitoringStepDto dto)
        {
            try
            {
                var addresses = _clientStationsRepository.GetClientsAddresses().Result;
                if (addresses == null)
                    return;
                _d2CWcfManager.SetClientsAddresses(addresses);
                _d2CWcfManager.NotifyUsersRtuCurrentMonitoringStep(dto).Wait();
            }
            catch (Exception e)
            {
                _logFile.AppendLine("WcfServiceForRtu.NotifyUserCurrentMonitoringStep: " + e.Message);
            }
        }

        public void RegisterRtuHeartbeat(RtuChecksChannelDto dto)
        {
            try
            {
                _rtuStationsRepository.RegisterRtuHeartbeatAsync(dto).Wait();
            }
            catch (Exception e)
            {
                    _logFile.AppendLine("WcfServiceForRtu.RegisterRtuHeartbeat: " + e.Message);
            }
        }
    }
}
