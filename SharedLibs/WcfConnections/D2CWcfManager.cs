using System;
using System.Collections.Generic;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.WcfConnections
{
    public class D2CWcfManager
    {
        private readonly List<DoubleAddress> _addresses;
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;

        public D2CWcfManager(List<DoubleAddress> addresses, IniFile iniFile, IMyLog logFile)
        {
            _addresses = addresses;
            _iniFile = iniFile;
            _logFile = logFile;
        }

        public void ConfirmClientRegistered(ClientRegisteredDto dto)
        {
            foreach (var clientAddress in _addresses)
            {
                var wcfConnection = new WcfFactory(clientAddress, _iniFile, _logFile).CreateClientConnection();
                if (wcfConnection == null)
                    continue;

                try
                {
                    wcfConnection.ConfirmClientRegistered(dto);
                    _logFile.AppendLine($"Sent registaration result to client {clientAddress.Main.ToStringA()}");
                }
                catch (Exception e)
                {
                    _logFile.AppendLine(e.Message);
                }
            }
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
                    _logFile.AppendLine($"Transfered response on check connection with RTU {dto.RtuId.First6()} to client {clientAddress.Main.ToStringA()}");
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
                    _logFile.AppendLine($"Transfered rtu command delivery confirmation: from RTU {dto.RtuId.First6()} to client {clientAddress.Main.ToStringA()}");
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
                    _logFile.AppendLine($"Transfered response on initialize from RTU {dto.Serial} to client {clientAddress.Main.ToStringA()}");
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
                    _logFile.AppendLine($"Transfered response on start monitoring from RTU {dto.RtuId.First6()} to client {clientAddress.Main.ToStringA()}");
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
                    _logFile.AppendLine($"Transfered response on stop monitoring from RTU {dto.RtuId.First6()} to client {clientAddress.Main.ToStringA()}");
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
                    _logFile.AppendLine($"Transfered response on apply monitoring settings from RTU {dto.RtuId.First6()} to client {clientAddress.Main.ToStringA()}");
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
//                    _logFile.AppendLine($"Transfered RTU {dto.RtuId.First6()} monitoring step to client {clientAddress.Main.ToStringA()}");
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
                    _logFile.AppendLine($"Transfered response on assign base ref from RTU {dto.RtuId.First6()} to client {clientAddress.Main.ToStringA()}");
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