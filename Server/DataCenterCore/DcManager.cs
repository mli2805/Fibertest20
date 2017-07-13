using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using DataCenterCore.ClientWcfServiceReference;
using DataCenterCore.RtuWcfServiceReference;
using Dto;
using Iit.Fibertest.Utils35;

namespace DataCenterCore
{
    public class DcManager
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

            lock (_rtuStationsLockObj)
            {
                InitializeRtuStationListFromDb();
            }

            lock (_clientStationsLockObj)
            {
                _clientStations = new List<ClientStation>();
            }
        }

        private List<RtuStation> InitializeRtuStationListFromDb()
        {
            var list = new List<RtuStation>();
            return list;
        }

        public bool CheckRtuConnection(CheckRtuConnectionDto dto)
        {
            var address = dto.IsAddressSetAsIp ? dto.Ip4Address : dto.HostName;
            var rtuWcfServiceClient = CreateAndOpenRtuWcfServiceClient(address);
            return rtuWcfServiceClient != null;
        }

        public bool InitializeRtu(InitializeRtuDto rtu)
        {
            var rtuWcfServiceClient = CreateAndOpenRtuWcfServiceClient(rtu.RtuIpAddress);
            if (rtuWcfServiceClient == null)
                return false;
            rtuWcfServiceClient.Initialize(rtu);

            _dcLog.AppendLine($"Transfered command to initialize RTU {rtu.Id} with ip={rtu.RtuIpAddress}");
            return true;
        }

        public bool ProcessRtuInitialized(RtuInitializedDto result)
        {
            var str = result.IsInitialized ? "OK" : "ERROR";
            _dcLog.AppendLine($"Rtu {result.Id} initialization {str}");
            ConfirmRtuInitialized(result);
            return true;
        }

        public bool ProcessMonitoringResult(MonitoringResult result)
        {
            _dcLog.AppendLine($"Monitoring result received. Sor size is {result.SorData.Length}");
            return true;
        }

        public void RegisterClient(string address)
        {
            _dcLog.AppendLine($"client {address} registration");
            lock (_clientStationsLockObj)
            {
                if (_clientStations.All(c => c.Ip != address))
                    _clientStations.Add(new ClientStation() { Ip = address });
            }
        }

        public void UnRegisterClient(string address)
        {
            _dcLog.AppendLine($"client {address} exited");
            lock (_clientStationsLockObj)
            {
                _clientStations.RemoveAll(c => c.Ip == address);
            }
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

        private void TransferConfirmRtuInitialized(string clientIp, RtuInitializedDto rtu)
        {
            var clientWcfServiceClient = CreateAndOpenClientWcfServiceClient(clientIp);
            if (clientWcfServiceClient == null)
                return;

            clientWcfServiceClient.ConfirmRtuInitialized(rtu);
            _dcLog.AppendLine($"Transfered initialization confirmation of RTU {rtu.Id} Serial={rtu.Serial}");
        }

        private void TransferConfirmStartMonitoring(string clientIp, MonitoringStartedDto confirmation)
        {
            var clientWcfServiceClient = CreateAndOpenClientWcfServiceClient(clientIp);
            if (clientWcfServiceClient == null)
                return;

            clientWcfServiceClient.ConfirmMonitoringStarted(confirmation);

            _dcLog.AppendLine($"Transfered start monitoring confirmation form RTU {confirmation.RtuId} result is {confirmation.IsSuccessful}");
        }

        private void TransferConfirmStopMonitoring(string clientIp, MonitoringStoppedDto confirmation)
        {
            var clientWcfServiceClient = CreateAndOpenClientWcfServiceClient(clientIp);
            if (clientWcfServiceClient == null)
                return;

            clientWcfServiceClient.ConfirmMonitoringStopped(confirmation);

            _dcLog.AppendLine($"Transfered stop monitoring confirmation form RTU {confirmation.RtuId} result is {confirmation.IsSuccessful}");
        }

        public bool StartMonitoring(string rtuAddress)
        {
            var rtuWcfServiceClient = CreateAndOpenRtuWcfServiceClient(rtuAddress);
            if (rtuWcfServiceClient == null)
                return false;

            rtuWcfServiceClient.StartMonitoring();
            _dcLog.AppendLine($"Transfered command to start monitoring for rtu with ip={rtuAddress}");
            return true;
        }

        public bool StopMonitoring(string rtuAddress)
        {
            var rtuWcfServiceClient = CreateAndOpenRtuWcfServiceClient(rtuAddress);
            if (rtuWcfServiceClient == null)
                return false;

            rtuWcfServiceClient.StopMonitoring();
            _dcLog.AppendLine($"Transfered command to stop monitoring for rtu with ip={rtuAddress}");
            return true;
        }

        private string CombineUriString(string address, int port, string wcfServiceName)
        {
            return @"net.tcp://" + address + @":" + port + @"/" + wcfServiceName;
        }

        private RtuWcfServiceClient CreateAndOpenRtuWcfServiceClient(string address)
        {
            try
            {
                var rtuWcfServiceClient = new RtuWcfServiceClient(CreateDefaultNetTcpBinding(), new EndpointAddress(new Uri(CombineUriString(address, 11842, @"RtuWcfService"))));
                rtuWcfServiceClient.Open();
                return rtuWcfServiceClient;
            }
            catch (Exception e)
            {
                _dcLog.AppendLine(e.Message);
                return null;
            }
        }

        private ClientWcfServiceClient CreateAndOpenClientWcfServiceClient(string address)
        {
            try
            {
                var clientWcfServiceClient = new ClientWcfServiceClient(CreateDefaultNetTcpBinding(), new EndpointAddress(new Uri(CombineUriString(address, 11843, @"ClientWcfService"))));
                clientWcfServiceClient.Open();
                return clientWcfServiceClient;
            }
            catch (Exception e)
            {
                _dcLog.AppendLine(e.Message);
                return null;
            }
        }

        private NetTcpBinding CreateDefaultNetTcpBinding()
        {
            return new NetTcpBinding
            {
                Security = { Mode = SecurityMode.None },
                ReceiveTimeout = new TimeSpan(0, 15, 0),
                SendTimeout = new TimeSpan(0, 15, 0),
                OpenTimeout = new TimeSpan(0, 1, 0),
                MaxBufferSize = 4096000 //4M
            };
        }

    }
}
