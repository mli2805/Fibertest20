﻿using System;
using System.Globalization;
using System.ServiceProcess;
using System.Threading;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.RtuWatchdog
{
    public class RtuWatch
    {
        private readonly IniFile _watchIniFile;
        private readonly IMyLog _watchLog;

        private string _rtuServiceName;
        private TimeSpan _maxGapBetweenMeasurements;
        private TimeSpan _maxGapBetweenAutoBaseMeasurements;
        private string _rtuManagerIniFile;

        public RtuWatch(IniFile watchIniFile, IMyLog watchLog)
        {
            _watchIniFile = watchIniFile;
            _watchLog = watchLog;
        }

        // ReSharper disable once FunctionNeverReturns -- as intended
        public void RunCycle()
        {
            GetPreliminarySettings();

            while (true)
            {
                try
                {
                    var sc = new ServiceController(_rtuServiceName);
                    if (sc.Status == ServiceControllerStatus.Stopped)
                    {
                        _watchLog.AppendLine($"{_rtuServiceName} is not running! Starting...");
                        sc.Start();
                        sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(10));
                        _watchIniFile.Write(IniSection.Watchdog, IniKey.LastRestartTime, DateTime.Now.ToString(CultureInfo.CurrentCulture));
                        continue;
                    }

                    if (IsLastRestartTimeParsed(out DateTime lastRestartTime)
                        && (IsGapBetweenMeasurementsExceeded(lastRestartTime) || IsGapBetweenAutoBaseMeasurementsExceeded(lastRestartTime)))
                        StopRtuManager(sc);
                }
                catch (Exception e)
                {
                    _watchLog.AppendLine(e.Message);
                }

                Thread.Sleep(TimeSpan.FromSeconds(3));
            }
        }

        private bool IsLastRestartTimeParsed(out DateTime lastRestartTime)
        {
            var lastRestartTimeStr = _watchIniFile.Read(IniSection.Watchdog, IniKey.LastRestartTime,
                DateTime.Now.ToString(CultureInfo.CurrentCulture));
            if (!DateTime.TryParse(lastRestartTimeStr, CultureInfo.CurrentCulture, DateTimeStyles.None,
                    out lastRestartTime))
            {
                _watchLog.AppendLine($"Cannot parse LastRestartTime {lastRestartTimeStr}");
                return false;
            }
            return true;
        }

        private bool IsGapBetweenMeasurementsExceeded(DateTime lastRestartTime)
        {
            if (DateTime.Now - lastRestartTime < _maxGapBetweenMeasurements)
                return false;

            var isMonitoringOnStr = _watchIniFile.ReadForeignIni(_rtuManagerIniFile, IniSection.Monitoring, IniKey.IsMonitoringOn);
            if (!bool.TryParse(isMonitoringOnStr, out bool isMonitoringOn))
            {
                _watchLog.AppendLine($"Cannot parse IsMonitoringOn {isMonitoringOnStr}");
                return false;
            }
            if (!isMonitoringOn) return false;

            var lastMeasurementTimeStr = _watchIniFile.ReadForeignIni(_rtuManagerIniFile, IniSection.Monitoring, IniKey.LastMeasurementTimestamp);
            if (!DateTime.TryParse(lastMeasurementTimeStr, CultureInfo.CurrentCulture, DateTimeStyles.None,
                out DateTime lastMeasurementTime))
            {
                _watchLog.AppendLine($"Cannot parse LastMeasurementTime {lastMeasurementTimeStr}");
                return false;
            }

            return DateTime.Now - lastMeasurementTime > _maxGapBetweenMeasurements;
        }

        private bool IsGapBetweenAutoBaseMeasurementsExceeded(DateTime lastRestartTime)
        {
            if (DateTime.Now - lastRestartTime < _maxGapBetweenAutoBaseMeasurements)
                return false;

            var isAutoBaseMeasurementInProgressStr = _watchIniFile.ReadForeignIni(_rtuManagerIniFile, IniSection.Monitoring, IniKey.IsAutoBaseMeasurementInProgress);
            if (!bool.TryParse(isAutoBaseMeasurementInProgressStr, out bool isAutoBaseMeasurementInProgress))
            {
                _watchLog.AppendLine($"Cannot parse IsMonitoringOn {isAutoBaseMeasurementInProgressStr}");
                return false;
            }
            if (!isAutoBaseMeasurementInProgress) return false;

            var lastAutoBaseMeasurementTimeStr = _watchIniFile.ReadForeignIni(_rtuManagerIniFile, IniSection.Monitoring, IniKey.LastAutoBaseMeasurementTimestamp);
            if (!DateTime.TryParse(lastAutoBaseMeasurementTimeStr, CultureInfo.CurrentCulture, DateTimeStyles.None,
                out DateTime lastAutoBaseMeasurementTime))
            {
                _watchLog.AppendLine($"Cannot parse LastMeasurementTime {lastAutoBaseMeasurementTimeStr}");
                return false;
            }

            return DateTime.Now - lastAutoBaseMeasurementTime > _maxGapBetweenAutoBaseMeasurements;
        }

        private void StopRtuManager(ServiceController sc)
        {
            _watchLog.AppendLine("Gap between measurements exceeded, stop RtuManager service...");
            sc.Stop();
            sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10));
        }

        private void GetPreliminarySettings()
        {
            _rtuServiceName = _watchIniFile.Read(IniSection.Watchdog, IniKey.RtuServiceName, "FibertestRtuService");

            var maxGapBetweenMeasurements =
                _watchIniFile.Read(IniSection.Watchdog, IniKey.MaxGapBetweenMeasurements, 600);
            _maxGapBetweenMeasurements = TimeSpan.FromSeconds(maxGapBetweenMeasurements);

            var maxGapBetweenAutoBaseMeasurements =
                _watchIniFile.Read(IniSection.Watchdog, IniKey.MaxGapBetweenAutoBaseMeasurements, 300);
            _maxGapBetweenAutoBaseMeasurements = TimeSpan.FromSeconds(maxGapBetweenAutoBaseMeasurements);

            _rtuManagerIniFile = Utils.FileNameForSure(@"..\Ini\", "RtuManager.ini", false);
        }
    }
}
