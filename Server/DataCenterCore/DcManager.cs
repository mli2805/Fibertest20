using System;
using System.Collections.Concurrent;
using Iit.Fibertest.DatabaseLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterCore
{
    public partial class DcManager
    {
        private readonly DoubleAddress _serverDoubleAddress;
        private readonly IMyLog _logFile;
        private readonly RtuRegistrationManager _rtuRegistrationManager;
        private readonly IniFile _iniFile;

        private ConcurrentDictionary<Guid, OldRtuStation> _rtuStations;

        public DcManager(IniFile iniFile, IMyLog logFile, RtuRegistrationManager rtuRegistrationManager)
        {
            _logFile = logFile;
            _rtuRegistrationManager = rtuRegistrationManager;
            _iniFile = iniFile;
            _serverDoubleAddress = _iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToRtu);
        }
    }
}