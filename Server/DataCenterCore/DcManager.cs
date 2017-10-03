using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForRtuInterface;
using WcfConnections;

namespace Iit.Fibertest.DataCenterCore
{
    public partial class DcManager
    {
        private readonly DoubleAddress _serverDoubleAddress;
        private readonly IMyLog _logFile;
        private readonly IniFile _iniFile;

        private ConcurrentDictionary<Guid, RtuStation> _rtuStations;
        private ConcurrentDictionary<Guid, ClientStation> _clientComps;
        private readonly object _clientStationsLockObj = new object();
        private List<ClientStation> _clientStations;

        public DcManager(IniFile iniFile, IMyLog logFile)
        {
            _logFile = logFile;
            _iniFile = iniFile;
            _serverDoubleAddress = _iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToRtu);
        }

        public void Start()
        {
            _clientComps = new ConcurrentDictionary<Guid, ClientStation>();
            _rtuStations = InitializeRtuStationListFromDb();
            _clientStations = new List<ClientStation>();

            StartWcfListenerToRtu();

            var lastConnectionTimeChecker =
                new LastConnectionTimeChecker(_logFile, _iniFile) { RtuStations = _rtuStations };
            var thread = new Thread(lastConnectionTimeChecker.Start) { IsBackground = true };
            thread.Start();
        }

        internal static ServiceHost ServiceForRtuHost;



        private void StartWcfListenerToRtu()
        {
            ServiceForRtuHost?.Close();

            WcfServiceForRtu.ServiceLog = _logFile;
            WcfServiceForRtu.MessageReceived += WcfServiceForRtu_MessageReceived;

            try
            {
                var uri = new Uri(WcfFactory.CombineUriString(@"localhost", (int)TcpPorts.ServerListenToRtu, @"WcfServiceForRtu"));

                ServiceForRtuHost = new ServiceHost(new WcfServiceForRtu(_rtuStations, _clientComps), uri);
                ServiceForRtuHost.AddServiceEndpoint(typeof(IWcfServiceForRtu),
                    WcfFactory.CreateDefaultNetTcpBinding(_iniFile), uri);
                ServiceForRtuHost.Open();
                _logFile.AppendLine("RTUs listener started successfully");
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
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
                _logFile.AppendLine("ReadDbTempTxt");
                _logFile.AppendLine(e.Message);
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
            _logFile.AppendLine($"{dictionary.Count} RTU found");
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
}