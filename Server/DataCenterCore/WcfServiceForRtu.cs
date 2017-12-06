using System;
using System.ServiceModel;
using System.Threading;
using Iit.Fibertest.DatabaseLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForRtuInterface;

namespace Iit.Fibertest.DataCenterCore
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class WcfServiceForRtu : IWcfServiceForRtu
    {
        private readonly IMyLog _logFile;
        private readonly RtuRegistrationManager _rtuRegistrationManager;
        private readonly RtuToClientsTransmitter _rtuToClientsTransmitter;

        public WcfServiceForRtu(IMyLog logFile,
            RtuRegistrationManager rtuRegistrationManager,
            RtuToClientsTransmitter rtuToClientsTransmitter)
        {
            _logFile = logFile;
            _rtuRegistrationManager = rtuRegistrationManager;
            _rtuToClientsTransmitter = rtuToClientsTransmitter;
        }

        public void NotifyUserCurrentMonitoringStep(CurrentMonitoringStepDto dto)
        {
            try
            {
                _rtuToClientsTransmitter.NotifyUsersRtuCurrentMonitoringStep(dto);
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
                _rtuRegistrationManager.RegisterRtuHeartbeatAsync(dto).Wait();
            }
            catch (Exception e)
            {
                    _logFile.AppendLine("WcfServiceForRtu.RegisterRtuHeartbeat: " + e.Message);
            }
        }
    }
}
