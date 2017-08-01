using System.Collections.Generic;
using Dto;
using Iit.Fibertest.Utils35;

namespace WcfConnections
{
    public class D2CWcfManager
    {
        private readonly List<string> _addresses;
        private readonly IniFile _iniFile;
        private readonly Logger35 _logger35;

        public D2CWcfManager(List<string> addresses, IniFile iniFile, Logger35 logger35)
        {
            _addresses = addresses;
            _iniFile = iniFile;
            _logger35 = logger35;
        }

        public void ConfirmRtuConnectionChecked(RtuConnectionCheckedDto dto)
        {
            foreach (var clientAddress in _addresses)
            {
                var wcfConnection = new WcfFactory(clientAddress, _iniFile, _logger35).CreateClientConnection();
                if (wcfConnection == null)
                    continue;

                wcfConnection.ConfirmRtuConnectionChecked(dto);
                _logger35.AppendLine($"Sent response on check connection with RTU {dto.RtuId}");
            }
        }

        public void ConfirmRtuInitialized(RtuInitializedDto dto)
        {
            foreach (var clientAddress in _addresses)
            {
                var wcfConnection = new WcfFactory(clientAddress, _iniFile, _logger35).CreateClientConnection();
                if (wcfConnection == null)
                    continue;

                wcfConnection.ConfirmRtuInitialized(dto);
                _logger35.AppendLine($"Sent response on initialize RTU {dto.Serial}");
            }
        }

        public void ConfirmMonitoringStarted(MonitoringStartedDto dto)
        {
            foreach (var clientAddress in _addresses)
            {
                var wcfConnection = new WcfFactory(clientAddress, _iniFile, _logger35).CreateClientConnection();
                if (wcfConnection == null)
                    continue;

                wcfConnection.ConfirmMonitoringStarted(dto);
                _logger35.AppendLine($"Sent response on start monitoring on RTU {dto.RtuId} to client {clientAddress}");
            }
        }

        public void ConfirmMonitoringStopped(MonitoringStoppedDto dto)
        {
            foreach (var clientAddress in _addresses)
            {
                var wcfConnection = new WcfFactory(clientAddress, _iniFile, _logger35).CreateClientConnection();
                if (wcfConnection == null)
                    continue;

                wcfConnection.ConfirmMonitoringStopped(dto);
                _logger35.AppendLine($"Sent response on stop monitoring on RTU {dto.RtuId}");
            }
        }
    }
}