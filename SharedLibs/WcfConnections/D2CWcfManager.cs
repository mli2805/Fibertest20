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
                    _logger35.AppendLine($"Transfered response on check connection with RTU {dto.RtuId} to client {clientAddress}");
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
                    _logger35.AppendLine($"Transfered rtu command delivery confirmation: from RTU {dto.RtuId} to client {clientAddress}");
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
                    _logger35.AppendLine($"Transfered response on initialize from RTU {dto.Serial} to client {clientAddress}");
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
                    _logger35.AppendLine($"Transfered response on start monitoring from RTU {dto.RtuId} to client {clientAddress}");
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
                    _logger35.AppendLine($"Transfered response on stop monitoring from RTU {dto.RtuId} to client {clientAddress}");
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
                    _logger35.AppendLine($"Transfered response on apply monitoring settings from RTU {dto.RtuId} to client {clientAddress}");
                }
                catch (Exception e)
                {
                    _logger35.AppendLine(e.Message);
                }
            }
            return true;
        }

        public bool ProcessRtuCurrentMonitoringStep(KnowRtuCurrentMonitoringStepDto dto)
        {
            foreach (var clientAddress in _addresses)
            {
                var wcfConnection = new WcfFactory(clientAddress, _iniFile, _logger35).CreateClientConnection();
                if (wcfConnection == null)
                    continue;

                try
                {
                    wcfConnection.ProcessRtuCurrentMonitoringStep(dto);
//                    _logger35.AppendLine($"Transfered RTU {dto.RtuId} monitoring step to client {clientAddress}");
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
                    _logger35.AppendLine($"Transfered response on assign base ref from RTU {dto.RtuId} to client {clientAddress}");
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