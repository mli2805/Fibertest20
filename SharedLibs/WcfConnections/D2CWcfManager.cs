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
                    _logger35.AppendLine($"Sent response on check connection with RTU {dto.RtuId}");
                }
                catch (Exception e)
                {
                    _logger35.AppendLine(e.Message);
                }
            }
        }

        public bool ConfirmRtuCommandDelivered(RtuCommandDeliveredDto dto)
        {
            foreach (var clientAddress in _addresses)
            {
                var wcfConnection = new WcfFactory(clientAddress, _iniFile, _logger35).CreateClientConnection();
                if (wcfConnection == null)
                    continue;

                try
                {
                    wcfConnection.ConfirmDelivery(dto);
                    _logger35.AppendLine($"Sent rtu command delivery confirmation: rtu {dto.RtuAddress}");
                }
                catch (Exception e)
                {
                    _logger35.AppendLine(e.Message);
                }
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

                try
                {
                    wcfConnection.ConfirmRtuInitialized(dto);
                    _logger35.AppendLine($"Sent response on initialize RTU {dto.Serial}");
                }
                catch (Exception e)
                {
                    _logger35.AppendLine(e.Message);
                }
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

                try
                {
                    wcfConnection.ConfirmMonitoringStarted(dto);
                    _logger35.AppendLine($"Sent response on start monitoring on RTU {dto.RtuId} to client {clientAddress}");
                }
                catch (Exception e)
                {
                    _logger35.AppendLine(e.Message);
                }
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

                try
                {
                    wcfConnection.ConfirmMonitoringStopped(dto);
                    _logger35.AppendLine($"Sent response on stop monitoring on RTU {dto.RtuId}");
                }
                catch (Exception e)
                {
                    _logger35.AppendLine(e.Message);
                }
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

                try
                {
                    wcfConnection.ConfirmMonitoringSettingsApplied(dto);
                    _logger35.AppendLine($"Sent response on apply monitoring settings on RTU {dto.RtuIpAddress}");
                }
                catch (Exception e)
                {
                    _logger35.AppendLine(e.Message);
                }
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

                try
                {
                    wcfConnection.ConfirmBaseRefAssigned(dto);
                    _logger35.AppendLine($"Sent response on assign base ref on RTU {dto.RtuIpAddress}");
                }
                catch (Exception e)
                {
                    _logger35.AppendLine(e.Message);
                }
            }
            return true;
        }
    }
}