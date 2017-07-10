﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
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

        public bool InitializeRtu(InitializeRtu rtu)
        {
            var rtuWcfServiceClient = CreateAndOpenRtuWcfServiceClient(rtu.RtuIpAddress);
            if (rtuWcfServiceClient == null)
                return false;
            rtuWcfServiceClient.Initialize(rtu);

            _dcLog.AppendLine($"Transfered command to initialize RTU {rtu.Id} with ip={rtu.RtuIpAddress}");
            return true;
        }

        public void ConfirmRtuInitialized(RtuInitialized rtu)
        {
            var list = new List<ClientStation>();
            lock (_clientStationsLockObj)
            {
                list.AddRange(_clientStations.Select(clientStation => (ClientStation) clientStation.Clone()));
            }
            foreach (var clientStation in list)
            {
                TransferConfirmRtuInitialized(clientStation.Ip);
            }
        }

        private void TransferConfirmRtuInitialized(string clientIp)
        {
            var clientWcfServiceClient = "";
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
//                _dcLog.AppendLine($@"Wcf client to {address} created");
                return rtuWcfServiceClient;
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
