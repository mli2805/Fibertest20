using System;
using System.Collections.Generic;
using Dto;
using Iit.Fibertest.UtilsLib;

namespace WcfConnections
{
    public class D2CWcfManager
    {
        private readonly List<DoubleAddressWithLastConnectionCheck> _addresses;
        private readonly IniFile _iniFile;
        private readonly LogFile _logFile;

        public D2CWcfManager(List<DoubleAddressWithLastConnectionCheck> addresses, IniFile iniFile, LogFile logFile)
        {
            _addresses = addresses;
            _iniFile = iniFile;
            _logFile = logFile;
        }

        public void ConfirmRtuConnectionChecked(RtuConnectionCheckedDto dto)
        {
            foreach (var clientAddress in _addresses)
            {
                var wcfConnection = new WcfFactory(clientAddress, _iniFile, _logFile).CreateClientConnection();
                if (wcfConnection == null)
                    continue;

                try
                {
                    wcfConnection.ConfirmRtuConnectionChecked(dto);
                    _logFile.AppendLine($"Transfered response on check connection with RTU {dto.RtuId} to client {clientAddress.Main}");
                }
                catch (Exception e)
                {
                    _logFile.AppendLine(e.Message);
                }
            }
        }

        public bool ConfirmRtuCommandDelivered(RtuCommandDeliveredDto dto)
        {
            foreach (var clientAddress in _addresses)
            {
                var wcfConnection = new WcfFactory(clientAddress, _iniFile, _logFile).CreateClientConnection();
                if (wcfConnection == null)
                    continue;

                try
                {
                    wcfConnection.ConfirmDelivery(dto);
                    _logFile.AppendLine($"Transfered rtu command delivery confirmation: from RTU {dto.RtuId} to client {clientAddress}");
                }
                catch (Exception e)
                {
                    _logFile.AppendLine(e.Message);
                }
            }
            return true;
        }

        public bool ConfirmRtuInitialized(RtuInitializedDto dto)
        {
            foreach (var clientAddress in _addresses)
            {
                var wcfConnection = new WcfFactory(clientAddress, _iniFile, _logFile).CreateClientConnection();
                if (wcfConnection == null)
                    continue;

                try
                {
                    wcfConnection.ConfirmRtuInitialized(dto);
                    _logFile.AppendLine($"Transfered response on initialize from RTU {dto.Serial} to client {clientAddress}");
                }
                catch (Exception e)
                {
                    _logFile.AppendLine(e.Message);
                }
            }
            return true;
        }

        public bool ConfirmMonitoringStarted(MonitoringStartedDto dto)
        {
            foreach (var clientAddress in _addresses)
            {
                var wcfConnection = new WcfFactory(clientAddress, _iniFile, _logFile).CreateClientConnection();
                if (wcfConnection == null)
                    continue;

                try
                {
                    wcfConnection.ConfirmMonitoringStarted(dto);
                    _logFile.AppendLine($"Transfered response on start monitoring from RTU {dto.RtuId} to client {clientAddress}");
                }
                catch (Exception e)
                {
                    _logFile.AppendLine(e.Message);
                }
            }
            return true;
        }

        public bool ConfirmMonitoringStopped(MonitoringStoppedDto dto)
        {
            foreach (var clientAddress in _addresses)
            {
                var wcfConnection = new WcfFactory(clientAddress, _iniFile, _logFile).CreateClientConnection();
                if (wcfConnection == null)
                    continue;

                try
                {
                    wcfConnection.ConfirmMonitoringStopped(dto);
                    _logFile.AppendLine($"Transfered response on stop monitoring from RTU {dto.RtuId} to client {clientAddress}");
                }
                catch (Exception e)
                {
                    _logFile.AppendLine(e.Message);
                }
            }
            return true;
        }

        public bool ConfirmMonitoringSettingsApplied(MonitoringSettingsAppliedDto dto)
        {
            foreach (var clientAddress in _addresses)
            {
                var wcfConnection = new WcfFactory(clientAddress, _iniFile, _logFile).CreateClientConnection();
                if (wcfConnection == null)
                    continue;

                try
                {
                    wcfConnection.ConfirmMonitoringSettingsApplied(dto);
                    _logFile.AppendLine($"Transfered response on apply monitoring settings from RTU {dto.RtuId} to client {clientAddress}");
                }
                catch (Exception e)
                {
                    _logFile.AppendLine(e.Message);
                }
            }
            return true;
        }

        public bool ProcessRtuCurrentMonitoringStep(KnowRtuCurrentMonitoringStepDto dto)
        {
            foreach (var clientAddress in _addresses)
            {
                var wcfConnection = new WcfFactory(clientAddress, _iniFile, _logFile).CreateClientConnection();
                if (wcfConnection == null)
                    continue;

                try
                {
                    wcfConnection.ProcessRtuCurrentMonitoringStep(dto);
//                    _logFile.AppendLine($"Transfered RTU {dto.RtuId} monitoring step to client {clientAddress}");
                }
                catch (Exception e)
                {
                    _logFile.AppendLine(e.Message);
                }
            }
            return true;
        }

        public bool ConfirmBaseRefAssigned(BaseRefAssignedDto dto)
        {
            foreach (var clientAddress in _addresses)
            {
                var wcfConnection = new WcfFactory(clientAddress, _iniFile, _logFile).CreateClientConnection();
                if (wcfConnection == null)
                    continue;

                try
                {
                    wcfConnection.ConfirmBaseRefAssigned(dto);
                    _logFile.AppendLine($"Transfered response on assign base ref from RTU {dto.RtuId} to client {clientAddress}");
                }
                catch (Exception e)
                {
                    _logFile.AppendLine(e.Message);
                }
            }
            return true;
        }
    }
}