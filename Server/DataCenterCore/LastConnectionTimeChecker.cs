using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Iit.Fibertest.UtilsLib;

namespace DataCenterCore
{
    public class LastConnectionTimeChecker
    {
        private readonly LogFile _dcLog;
        private readonly IniFile _coreIni;
        public ConcurrentDictionary<Guid, RtuStation> RtuStations { get; set; }

        public LastConnectionTimeChecker(LogFile dcLog, IniFile coreIni)
        {
            _dcLog = dcLog;
            _coreIni = coreIni;
        }

        public void Start()
        {
            var checkRtuIsAliveTimeout =
                TimeSpan.FromSeconds(_coreIni.Read(IniSection.General, IniKey.CheckRtuIsAliveTimeout, 1));
            var permittedTimeBetweenConnection =
                TimeSpan.FromSeconds(_coreIni.Read(IniSection.General, IniKey.PermittedTimeBetweenConnection, 70));

            while (true)
            {
                foreach (var rtuStation in RtuStations)
                {
                    CheckLastConnectionOnMainChannelTime(rtuStation, permittedTimeBetweenConnection);
                    if (rtuStation.Value.PcAddresses.DoubleAddress.HasReserveAddress)
                        CheckConnectionOnReserveChannelTime(rtuStation, permittedTimeBetweenConnection);
                }

                Thread.Sleep(checkRtuIsAliveTimeout);
            }

        }

        private void CheckConnectionOnReserveChannelTime(KeyValuePair<Guid, RtuStation> rtuStation, TimeSpan permittedTimeBetweenConnection)
        {
            var isTimeoutExceeded = DateTime.Now - rtuStation.Value.PcAddresses.LastConnectionOnReserve >
                                    permittedTimeBetweenConnection;
            if (!isTimeoutExceeded && rtuStation.Value.PcAddresses.IsLastCheckOfReserveSuccessfull == false)
                _dcLog.AppendLine($"RTU {rtuStation.Key.First6()} established connection by reserve channel");
            if (isTimeoutExceeded && rtuStation.Value.PcAddresses.IsLastCheckOfReserveSuccessfull == true)
                _dcLog.AppendLine($"Alarm! RTU {rtuStation.Key.First6()} reserve channel check-in timeout exceeded");
            rtuStation.Value.PcAddresses.IsLastCheckOfReserveSuccessfull = !isTimeoutExceeded;
        }

        private void CheckLastConnectionOnMainChannelTime(KeyValuePair<Guid, RtuStation> rtuStation, TimeSpan permittedTimeBetweenConnection)
        {
            var isTimeoutExceeded = (DateTime.Now - rtuStation.Value.PcAddresses.LastConnectionOnMain) > permittedTimeBetweenConnection;
            if (!isTimeoutExceeded && rtuStation.Value.PcAddresses.IsLastCheckOfMainSuccessfull == false)
                _dcLog.AppendLine($"RTU {rtuStation.Key.First6()} established connection by main channel");
            if (isTimeoutExceeded && rtuStation.Value.PcAddresses.IsLastCheckOfMainSuccessfull == true)
                _dcLog.AppendLine($"Alarm! RTU {rtuStation.Key.First6()} main channel check-in timeout exceeded");
            rtuStation.Value.PcAddresses.IsLastCheckOfMainSuccessfull = !isTimeoutExceeded;
        }
    }
}
