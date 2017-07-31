using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using System.ServiceModel;
using Dto;
using Iit.Fibertest.Utils35;
using WcfServiceForClientLibrary;
using WcfServiceForRtuLibrary;

namespace DataCenterCore
{
    public partial class DcManager
    {
        private readonly Logger35 _dcLog;
        private readonly IniFile _coreIni;

        private readonly object _rtuStationsLockObj = new object();
        private List<RtuStation> _rtuStations;

        private readonly object _clientStationsLockObj = new object();
        private List<ClientStation> _clientStations;


        public DcManager()
        {
            _coreIni = new IniFile();
            _coreIni.AssignFile("DcCore.ini");
            var cultureString = _coreIni.Read(IniSection.General, IniKey.Culture, "ru-RU");

            _dcLog = new Logger35();
            _dcLog.AssignFile("DcCore.log", cultureString);
            _dcLog.EmptyLine();
            _dcLog.EmptyLine('-');


            lock (_rtuStationsLockObj)
            {
                InitializeRtuStationListFromDb();
            }

            lock (_clientStationsLockObj)
            {
                _clientStations = new List<ClientStation>();
            }

            StartWcfListenerToClient();
            StartWcfListenerToRtu();
        }

        internal static ServiceHost ServiceForRtuHost;
        internal static ServiceHost ServiceForClientHost;

        private void StartWcfListenerToRtu()
        {
            ServiceForRtuHost?.Close();

            WcfServiceForRtu.ServiceLog = _dcLog;
            WcfServiceForRtu.MessageReceived += WcfServiceForRtu_MessageReceived;
            ServiceForRtuHost = new ServiceHost(typeof(WcfServiceForRtu));
            try
            {
                ServiceForRtuHost.Open();
                _dcLog.AppendLine("RTUs listener started successfully");
            }
            catch (Exception e)
            {
                _dcLog.AppendLine(e.Message);
                throw;
            }
        }

        private void WcfServiceForRtu_MessageReceived(object msg)
        {
            var dto = msg as RtuInitializedDto;
            if (dto != null)
            {
                ProcessRtuInitialized(dto);
                return;
            }
            var dto2 = msg as MonitoringStartedDto;
            if (dto2 != null)
            {
                ProcessMonitoringStarted(dto2);
                return;
            }
            var dto3 = msg as MonitoringStoppedDto;
            if (dto3 != null)
            {
                ProcessMonitoringStopped(dto3);
            }
        }

        private void StartWcfListenerToClient()
        {
            ServiceForClientHost?.Close();

            WcfServiceForClient.ServiceLog = _dcLog;
            WcfServiceForClient.MessageReceived += WcfServiceForClient_MessageReceived;
            ServiceForClientHost = new ServiceHost(typeof(WcfServiceForClient));
            try
            {
                ServiceForClientHost.Open();
                _dcLog.AppendLine("Clients listener started successfully");
            }
            catch (Exception e)
            {
                _dcLog.AppendLine(e.Message);
                throw;
            }
        }

        private List<RtuStation> InitializeRtuStationListFromDb()
        {
            var list = new List<RtuStation>();
            return list;
        }

        public bool ProcessRtuInitialized(RtuInitializedDto result)
        {
            var str = result.IsInitialized ? "OK" : "ERROR";
            _dcLog.AppendLine($"Rtu {result.Id} initialization {str}");
            ConfirmRtuInitialized(result);
            return true;
        }

        public bool ProcessMonitoringStarted(MonitoringStartedDto result)
        {
            _dcLog.AppendLine($"Rtu {result.RtuId} monitoring started: {result.IsSuccessful}");
            ConfirmStartMonitoring(result);
            return true;
        }

        public bool ProcessMonitoringStopped(MonitoringStoppedDto result)
        {
            _dcLog.AppendLine($"Rtu {result.RtuId} monitoring stopped: {result.IsSuccessful}");
            ConfirmStopMonitoring(result);
            return true;
        }


        public bool ProcessMonitoringResult(MonitoringResult result)
        {
            _dcLog.AppendLine($"Monitoring result received. Sor size is {result.SorData.Length}");
            return true;
        }


        public void ConfirmRtuInitialized(RtuInitializedDto rtu)
        {
            var list = new List<ClientStation>();
            lock (_clientStationsLockObj)
            {
                list.AddRange(_clientStations.Select(clientStation => (ClientStation)clientStation.Clone()));
            }
            foreach (var clientStation in list)
            {
                TransferConfirmRtuInitialized(clientStation.Ip, rtu);
            }
        }

        public bool ConfirmStartMonitoring(MonitoringStartedDto confirmation)
        {
            var list = new List<ClientStation>();
            lock (_clientStationsLockObj)
            {
                list.AddRange(_clientStations.Select(clientStation => (ClientStation)clientStation.Clone()));
            }
            foreach (var clientStation in list)
            {
                TransferConfirmStartMonitoring(clientStation.Ip, confirmation);
            }
            return true;
        }

        public bool ConfirmStopMonitoring(MonitoringStoppedDto confirmation)
        {
            var list = new List<ClientStation>();
            lock (_clientStationsLockObj)
            {
                list.AddRange(_clientStations.Select(clientStation => (ClientStation)clientStation.Clone()));
            }
            foreach (var clientStation in list)
            {
                TransferConfirmStopMonitoring(clientStation.Ip, confirmation);
            }
            return true;
        }

        public bool ConfirmApplyMonitoringSettings(MonitoringSettingsAppliedDto confirmation)
        {
            var list = new List<ClientStation>();
            lock (_clientStationsLockObj)
            {
                list.AddRange(_clientStations.Select(clientStation => (ClientStation)clientStation.Clone()));
            }
            foreach (var clientStation in list)
            {
                TransferConfirmMonitoringSettings(clientStation.Ip, confirmation);
            }
            return true;
        }

        public bool ConfirmAssignBaseRef(BaseRefAssignedDto confirmation)
        {
            var list = new List<ClientStation>();
            lock (_clientStationsLockObj)
            {
                list.AddRange(_clientStations.Select(clientStation => (ClientStation)clientStation.Clone()));
            }
            foreach (var clientStation in list)
            {
                TransferConfirmBaseRef(clientStation.Ip, confirmation);
            }
            return true;
        }


        private void TransferRtuConnectionChecked(string clientIp, RtuConnectionCheckedDto dto)
        {
            _dcLog.AppendLine("TransferRtuConnectionChecked");
            var clientConnection = new WcfConnections.WcfFactory(clientIp, _coreIni, _dcLog).CreateClientConnection();
            if (clientConnection == null)
            {
                _dcLog.AppendLine($"Cannot transfer RTU {dto.RtuId} check connection result to client {clientIp}");
                return;
            }
            _dcLog.AppendLine("connected");
            clientConnection.ConfirmRtuConnectionChecked(dto);
            _dcLog.AppendLine($"Transfered RTU {dto.RtuId} check connection result");
        }
        private void TransferConfirmRtuInitialized(string clientIp, RtuInitializedDto rtu)
        {
            var clientConnection = new WcfConnections.WcfFactory(clientIp, _coreIni, _dcLog).CreateClientConnection();
            if (clientConnection == null)
            {
                _dcLog.AppendLine($"Cannot transfer initialization confirmation of RTU {rtu.Id} Serial={rtu.Serial}");
                return;
            }

            clientConnection.ConfirmRtuInitialized(rtu);
            _dcLog.AppendLine($"Transfered initialization confirmation of RTU {rtu.Id} Serial={rtu.Serial}");
        }
        private void TransferConfirmStartMonitoring(string clientIp, MonitoringStartedDto confirmation)
        {
            var clientConnection = new WcfConnections.WcfFactory(clientIp, _coreIni, _dcLog).CreateClientConnection();
            if (clientConnection == null)
            {
                return;
            }

            clientConnection.ConfirmMonitoringStarted(confirmation);

            _dcLog.AppendLine($"Transfered start monitoring confirmation from RTU {confirmation.RtuId} result is {confirmation.IsSuccessful}");
        }
        private void TransferConfirmStopMonitoring(string clientIp, MonitoringStoppedDto confirmation)
        {
            var clientConnection = new WcfConnections.WcfFactory(clientIp, _coreIni, _dcLog).CreateClientConnection();
            if (clientConnection == null)
            {
                return;
            }

            clientConnection.ConfirmMonitoringStopped(confirmation);

            _dcLog.AppendLine($"Transfered stop monitoring confirmation from RTU {confirmation.RtuId} result is {confirmation.IsSuccessful}");
        }

        private void TransferConfirmMonitoringSettings(string clientIp, MonitoringSettingsAppliedDto confirmation)
        {
            var clientConnection = new WcfConnections.WcfFactory(clientIp, _coreIni, _dcLog).CreateClientConnection();
            if (clientConnection == null)
            {
                return;
            }

            clientConnection.ConfirmMonitoringSettingsApplied(confirmation);
            _dcLog.AppendLine($"Transfered apply monitoring settings confirmation from RTU {confirmation.RtuIpAddress} result is {confirmation.IsSuccessful}");
        }

        private void TransferConfirmBaseRef(string clientIp, BaseRefAssignedDto confirmation)
        {
            var clientConnection = new WcfConnections.WcfFactory(clientIp, _coreIni, _dcLog).CreateClientConnection();
            if (clientConnection == null)
            {
                return;
            }

            clientConnection.ConfirmBaseRefAssigned(confirmation);
            _dcLog.AppendLine($"Transfered apply monitoring settings confirmation from RTU {confirmation.RtuIpAddress} result is {confirmation.IsSuccessful}");
        }

    }
}
