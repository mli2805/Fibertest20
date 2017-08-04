using System;
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

                try
                {
                    wcfConnection.ConfirmRtuConnectionChecked(dto);
                }
                catch (Exception e)
                {
                    _logger35.AppendLine(e.Message);
                }
                _logger35.AppendLine($"Sent response on check connection with RTU {dto.RtuId}");
            }
        }

        public bool ConfirmRtuCommandDelivered(RtuCommandDeliveredDto dto)
        {
            foreach (var clientAddress in _addresses)
            {
                var wcfConnection = new WcfFactory(clientAddress, _iniFile, _logger35).CreateClientConnection();
                if (wcfConnection == null)
                    continue;

                wcfConnection.ConfirmDelivery(dto);
                _logger35.AppendLine($"Sent rtu command delivery confirmation: rtu {dto.RtuAddress}");
            }
            return true;
        }

        public bool ConfirmRtuInitialized(RtuInitializedDto dto)
        {
            foreach (var clientAddress in _addresses)
            {
                var wcfConnection = new WcfFactory(clientAddress, _iniFile, _logger35).CreateClientConnection();
                if (wcfConnection == null)
                    continue;

                wcfConnection.ConfirmRtuInitialized(dto);
                _logger35.AppendLine($"Sent response on initialize RTU {dto.Serial}");
            }
            return true;
        }

        public bool ConfirmMonitoringStarted(MonitoringStartedDto dto)
        {
            foreach (var clientAddress in _addresses)
            {
                var wcfConnection = new WcfFactory(clientAddress, _iniFile, _logger35).CreateClientConnection();
                if (wcfConnection == null)
                    continue;

                wcfConnection.ConfirmMonitoringStarted(dto);
                _logger35.AppendLine($"Sent response on start monitoring on RTU {dto.RtuId} to client {clientAddress}");
            }
            return true;
        }

        public bool ConfirmMonitoringStopped(MonitoringStoppedDto dto)
        {
            foreach (var clientAddress in _addresses)
            {
                var wcfConnection = new WcfFactory(clientAddress, _iniFile, _logger35).CreateClientConnection();
                if (wcfConnection == null)
                    continue;

                wcfConnection.ConfirmMonitoringStopped(dto);
                _logger35.AppendLine($"Sent response on stop monitoring on RTU {dto.RtuId}");
            }
            return true;
        }

        public bool ConfirmMonitoringSettingsApplied(MonitoringSettingsAppliedDto dto)
        {
            foreach (var clientAddress in _addresses)
            {
                var wcfConnection = new WcfFactory(clientAddress, _iniFile, _logger35).CreateClientConnection();
                if (wcfConnection == null)
                    continue;

                wcfConnection.ConfirmMonitoringSettingsApplied(dto);
                _logger35.AppendLine($"Sent response on apply monitoring settings on RTU {dto.RtuIpAddress}");
            }
            return true;
        }

        public bool ConfirmBaseRefAssigned(BaseRefAssignedDto dto)
        {
            foreach (var clientAddress in _addresses)
            {
                var wcfConnection = new WcfFactory(clientAddress, _iniFile, _logger35).CreateClientConnection();
                if (wcfConnection == null)
                    continue;

                wcfConnection.ConfirmBaseRefAssigned(dto);
                _logger35.AppendLine($"Sent response on assign base ref on RTU {dto.RtuIpAddress}");
            }
            return true;
        }
    }
}