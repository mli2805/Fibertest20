using System;
using System.Collections.Concurrent;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterCore
{
    public partial class DcManager
    {
        private readonly DoubleAddress _serverDoubleAddress;
        private readonly IMyLog _logFile;
        private readonly IniFile _iniFile;

        private ConcurrentDictionary<Guid, RtuStation> _rtuStations;

        public DcManager(IniFile iniFile, IMyLog logFile)
        {
            _logFile = logFile;
            _iniFile = iniFile;
            _serverDoubleAddress = _iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToRtu);
        }
    }
}