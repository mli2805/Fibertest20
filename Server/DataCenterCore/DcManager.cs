using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using Dto;
using Iit.Fibertest.UtilsLib;
using WcfConnections;
using WcfServiceForClientLibrary;
using WcfServiceForRtuLibrary;

namespace DataCenterCore
{
    public partial class DcManager
    {
        private readonly DoubleAddress _serverDoubleAddress;
        private readonly IMyLog _dcLog;
        private readonly IniFile _coreIni;

        private readonly ConcurrentDictionary<Guid, RtuStation> _rtuStations;

        private readonly object _clientStationsLockObj = new object();
        private readonly List<ClientStation> _clientStations;

        public DcManager(DoubleAddress serverDoubleAddress)
        {
            _serverDoubleAddress = serverDoubleAddress;
            _coreIni = new IniFile();
            _coreIni.AssignFile("DcCore.ini");
            var cultureString = _coreIni.Read(IniSection.General, IniKey.Culture, "ru-RU");
            var logFileSizeLimit = _coreIni.Read(IniSection.General, IniKey.LogFileSizeLimitKb, 0);

            _dcLog = new LogFile();
            _dcLog.AssignFile("DcCore.log", logFileSizeLimit, cultureString);
            _dcLog.EmptyLine();
            _dcLog.EmptyLine('-');

            _rtuStations = InitializeRtuStationListFromDb();

            lock (_clientStationsLockObj)
            {
                _clientStations = new List<ClientStation>();
            }

            StartWcfListenerToClient();
            StartWcfListenerToRtu();

            var lastConnectionTimeChecker = new LastConnectionTimeChecker(_dcLog, _coreIni);
            lastConnectionTimeChecker.RtuStations = _rtuStations;
            var thread = new Thread(lastConnectionTimeChecker.Start) { IsBackground = true };
            thread.Start();
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
                ServiceForRtuHost.AddServiceEndpoint(typeof(IWcfServiceForRtu),
                    WcfFactory.CreateDefaultNetTcpBinding(_coreIni),
                    WcfFactory.CombineUriString(@"localhost", (int)TcpPorts.ServerListenToRtu, @"WcfServiceForRtu"));
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
                ServiceForClientHost.AddServiceEndpoint(typeof(IWcfServiceForClient),
                    WcfFactory.CreateDefaultNetTcpBinding(_coreIni),
                    WcfFactory.CombineUriString(@"localhost", (int)TcpPorts.ServerListenToClient, @"WcfServiceForClient"));
                ServiceForClientHost.Open();
                _dcLog.AppendLine("Clients listener started successfully");
            }
            catch (Exception e)
            {
                _dcLog.AppendLine(e.Message);
                throw;
            }
        }

        private ConcurrentDictionary<Guid, RtuStation> InitializeRtuStationListFromDb()
        {
            try
            {
                return ReadDbTempTxt();

            }
            catch (Exception e)
            {
                _dcLog.AppendLine("ReadDbTempTxt");
                _dcLog.AppendLine(e.Message);
                return null;
            }
        }

        #region Temporary functions for store rtu in txt file

        private ConcurrentDictionary<Guid, RtuStation> ReadDbTempTxt()
        {
            var dictionary = new ConcurrentDictionary<Guid, RtuStation>();

            var app = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var path = Path.GetDirectoryName(app);
            if (path == null)
                return dictionary;
            var filename = Path.Combine(path, @"..\Ini\DbTemp.txt");
            if (File.Exists(filename))
            {
                var content = File.ReadAllLines(filename);
                foreach (var line in content)
                {
                    var rtuStation = ParseLine(line);
                    dictionary.TryAdd(rtuStation.Id, rtuStation);
                }
            }
            _dcLog.AppendLine($"{dictionary.Count} RTU found");
            return dictionary;
        }

        private void WriteDbTempTxt()
        {
            var app = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var path = Path.GetDirectoryName(app);
            if (path == null)
                return;
            var filename = Path.Combine(path, @"..\Ini\DbTemp.txt");
            if (File.Exists(filename))
                File.Delete(filename);

            var content = _rtuStations.Select(rtuStation => RtuStationToLine(rtuStation.Value)).ToList();
            File.WriteAllLines(filename, content);
        }

        private static RtuStation ParseLine(string line)
        {
            var parts = line.Split(' ');
            var rtuStation = new RtuStation()
            {
                Id = Guid.Parse(parts[0]),
                PcAddresses = new DoubleAddressWithLastConnectionCheck()
                {
                    DoubleAddress = new DoubleAddress()
                    {
                        Main = new NetAddress(parts[1], (int)TcpPorts.RtuListenTo),
                    },
                    LastConnectionOnMain = DateTime.Now,
                },
            };
            if (parts[2] != @"none")
            {
                rtuStation.PcAddresses.DoubleAddress.HasReserveAddress = true;
                rtuStation.PcAddresses.DoubleAddress.Reserve = new NetAddress(parts[2], (int)TcpPorts.RtuListenTo);
                rtuStation.PcAddresses.LastConnectionOnReserve = DateTime.Now;
            }
            rtuStation.OtdrIp = parts[3];
            return rtuStation;
        }

        private static string RtuStationToLine(RtuStation rtuStation)
        {
            var reserveAddress = rtuStation.PcAddresses.DoubleAddress.HasReserveAddress
                ? rtuStation.PcAddresses.DoubleAddress.Reserve.Ip4Address
                : "none";
            return $"{rtuStation.Id} {rtuStation.PcAddresses.DoubleAddress.Main.Ip4Address} {reserveAddress} {rtuStation.OtdrIp}";
        }

        #endregion
    }

    public interface IWcfServiceForClientHost
    {
        void StartWcfListenerToClient();
    }
    public sealed class WcfServiceForClientHost : IWcfServiceForClientHost
    {
        private readonly ServiceHost _wcfHost = new ServiceHost(typeof(WcfServiceForClient));
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;

        public WcfServiceForClientHost(IniFile iniFile, IMyLog logFile)
        {
            _logFile = logFile;
            _iniFile = iniFile;
        }

        public void StartWcfListenerToClient()
        {
            throw new NotImplementedException();
        }
    }
}