using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.WcfConnections
{
    public class D2CWcfManager
    {
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;

        private List<DoubleAddress> _addresses;

        public D2CWcfManager(IniFile iniFile, IMyLog logFile)
        {
            _iniFile = iniFile;
            _logFile = logFile;
        }

        public void SetClientsAddresses(List<DoubleAddress> addresses)
        {
            _addresses = addresses;
        }

        public async Task<int> NotifyUsersRtuCurrentMonitoringStep(CurrentMonitoringStepDto dto)
        {
            if (_addresses == null)
            {
                _logFile.AppendLine("There are no clients, who are you sending to?");
                return 0;
            }
            foreach (var clientAddress in _addresses)
            {
                var wcfConnection = new WcfFactory(clientAddress, _iniFile, _logFile).CreateClientConnection();
                if (wcfConnection == null)
                    continue;

                try
                {
                    await wcfConnection.NotifyUsersRtuCurrentMonitoringStep(dto);
                }
                catch (Exception e)
                {
                    _logFile.AppendLine("D2CWcfManager.NotifyUsersRtuCurrentMonitoringStep: " + e.Message);
                }
            }
            return 0;
        }

        public async Task<int> NotifyAboutRtuChangedAvailability(ListOfRtuWithChangedAvailabilityDto dto)
        {
            if (_addresses == null)
            {
                _logFile.AppendLine("There are no clients, who are you sending to?");
                return 0;
            }
            foreach (var clientAddress in _addresses)
            {
                var wcfConnection = new WcfFactory(clientAddress, _iniFile, _logFile).CreateClientConnection();
                if (wcfConnection == null)
                    continue;

                try
                {
                    await wcfConnection.NotifyAboutRtuChangedAvailability(dto);
                }
                catch (Exception e)
                {
                    _logFile.AppendLine(e.Message);
                }
            }
            return 0;
        }

        public async Task<int> NotifyAboutMonitoringResult(MonitoringResultDto dto)
        {
            if (_addresses == null)
            {
                _logFile.AppendLine("There are no clients, who are you sending to?");
                return 0;
            }
            foreach (var clientAddress in _addresses)
            {
                var wcfConnection = new WcfFactory(clientAddress, _iniFile, _logFile).CreateClientConnection();
                if (wcfConnection == null)
                    continue;

                try
                {
                    await wcfConnection.NotifyAboutMonitoringResult(dto);
                }
                catch (Exception e)
                {
                    _logFile.AppendLine(e.Message);
                }
            }
            return 0;
        }



    }
}