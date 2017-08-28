using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using Dto;
using Iit.Fibertest.UtilsLib;
using WcfServiceForClientLibrary;
using WcfServiceForRtuLibrary;

namespace DataCenterCore
{
    public partial class DcManager
    {
        private readonly LogFile _dcLog;
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
            var logFileSizeLimit = _coreIni.Read(IniSection.General, IniKey.LogFileSizeLimitKb, 0);

            _dcLog = new LogFile();
            _dcLog.AssignFile("DcCore.log", logFileSizeLimit, cultureString);
            _dcLog.EmptyLine();
            _dcLog.EmptyLine('-');

            lock (_rtuStationsLockObj)
            {
                _rtuStations = InitializeRtuStationListFromDb();
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
            return ReadDbTempTxt();
        }

        private List<RtuStation> ReadDbTempTxt()
        {
            var list = new List<RtuStation>();

            var app = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var path = Path.GetDirectoryName(app);
            if (path == null)
                return null;
            var filename = Path.Combine(path, @"..\Ini\DbTemp.txt");
            if (File.Exists(filename))
            {
                var content = File.ReadAllLines(filename);
                list = content.Select(ParseLine).ToList();
            }
            _dcLog.AppendLine($"{list.Count} RTU found");
            return list;
        }

        private static RtuStation ParseLine(string line)
        {
            var parts = line.Split(' ');
            var rtuStation = new RtuStation()
            {
                Id = Guid.Parse(parts[0]),
                PcAddresses = new DoubleAddressWithLastConnectionCheck()
                {
                    Main = new NetAddress(parts[1], (int)TcpPorts.RtuListenTo),
                    LastConnectionOnMain = DateTime.Now,
                },
            };
            if (parts[2] != @"none")
            {
                rtuStation.PcAddresses.HasReserveAddress = true;
                rtuStation.PcAddresses.Reserve = new NetAddress(parts[2], (int)TcpPorts.RtuListenTo);
                rtuStation.PcAddresses.LastConnectionOnReserve = DateTime.Now;
            }
            rtuStation.CharonIp = parts[3];
            return rtuStation;
        }
    }
}