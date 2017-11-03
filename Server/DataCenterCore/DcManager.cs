using System;
using System.Collections.Concurrent;
using System.Threading;
using Iit.Fibertest.DatabaseLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterCore
{
    public partial class DcManager
    {
        private readonly DoubleAddress _serverDoubleAddress;
        private readonly IMyLog _logFile;
        private readonly DbManager _dbManager;
        private readonly ClientRegistrationManager _clientRegistrationManager;
        private readonly IniFile _iniFile;

        private ConcurrentDictionary<Guid, RtuStation> _rtuStations;

        public DcManager(IniFile iniFile, IMyLog logFile, DbManager dbManager, ClientRegistrationManager clientRegistrationManager)
        {
            _logFile = logFile;
            _dbManager = dbManager;
            _clientRegistrationManager = clientRegistrationManager;
            _iniFile = iniFile;
            _serverDoubleAddress = _iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToRtu);
        }

        public void Start(ConcurrentDictionary<Guid, RtuStation> rtuStations)
        {
            _rtuStations = rtuStations;
            foreach (var rtuStation in rtuStations)
            {
                _logFile.AppendLine($"{rtuStation.Value.Id.First6()} {rtuStation.Value.PcAddresses.DoubleAddress.Main.ToStringA()}");
            }
            _logFile.AppendLine($"{_rtuStations.Count} RTU found");

            var lastConnectionTimeChecker =
                new LastConnectionTimeChecker(_logFile, _iniFile) { RtuStations = _rtuStations };
            var thread = new Thread(lastConnectionTimeChecker.Start) { IsBackground = true };
            thread.Start();
        }
        
    }
}