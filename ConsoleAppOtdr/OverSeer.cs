using System;
using System.Collections.Generic;
using System.IO;
using Iit.Fibertest.DirectCharonLibrary;
using Iit.Fibertest.IitOtdrLibrary;
using Iit.Fibertest.Utils35;
using Iit.Fibertest.Utils35.IniFile;

namespace ConsoleAppOtdr
{
    public class OverSeer
    {
        private const string DefaultIp = "192.168.88.101";

        private readonly Logger35 _logger35;
        private readonly IniFile _iniFile35;
        private OtdrManager _otdrManager;
        private Charon _mainCharon;

        private Queue<ExtendedPort> _monitoringQueue;
        private int _measurementNumber;
        private TimeSpan _preciseMakeTimespan;
        private TimeSpan _preciseSaveTimespan;
        private TimeSpan _fastSaveTimespan;

        public OverSeer(Logger35 logger35, IniFile iniFile35)
        {
            _logger35 = logger35;
            _iniFile35 = iniFile35;
        }

        public bool InitializeOtdr()
        {
            _otdrManager = new OtdrManager(@"OtdrMeasEngine\", _iniFile35, _logger35);
            if (_otdrManager.LoadDll() != "")
                return false;

            var otdrAddress = _iniFile35.Read(IniSection.General, IniKey.OtdrIp, DefaultIp);
            if (_otdrManager.InitializeLibrary())
                _otdrManager.ConnectOtdr(otdrAddress);
            return _otdrManager.IsOtdrConnected;
        }

        public bool InitializeOtau()
        {
            var otauIpAddress = _iniFile35.Read(IniSection.General, IniKey.OtauIp, DefaultIp);
            _mainCharon = new Charon(new NetAddress(otauIpAddress, 23), _iniFile35, _logger35);
            return _mainCharon.Initialize();
        }

        public void GetMonitoringQueue()
        {
            _monitoringQueue = new Queue<ExtendedPort>();
            var monitoringSettingsFile = Utils.FileNameForSure(@"..\ini\", @"monitoring.que", false);
            var content = File.ReadAllLines(monitoringSettingsFile);
            foreach (var line in content)
            {
                var extendedPort = Create(line);
                if (extendedPort != null && _mainCharon.IsExtendedPortValidForMonitoring(extendedPort))
                    _monitoringQueue.Enqueue(extendedPort);
            }
        }

        public void GetMonitoringParams()
        {
            _preciseMakeTimespan = TimeSpan.FromSeconds(_iniFile35.Read(IniSection.Monitoring, IniKey.PreciseMakeTimespan, 3600));
            _preciseSaveTimespan = TimeSpan.FromSeconds(_iniFile35.Read(IniSection.Monitoring, IniKey.PreciseSaveTimespan, 3600));
            _fastSaveTimespan = TimeSpan.FromSeconds(_iniFile35.Read(IniSection.Monitoring, IniKey.FastSaveTimespan, 3600));
        }

        private ExtendedPort Create(string str)
        {
            if (string.IsNullOrEmpty(str))
                return null;

            var parts = str.Split('-');
            if (parts.Length != 2)
            {
                _logger35.AppendLine($"Invalid string in queue file: {str}");
                return null;
            }

            int opticalPort;
            if (!int.TryParse(parts[1], out opticalPort))
            {
                _logger35.AppendLine($"Can't parse optical port: {parts[1]}");
                return null;
            }

            var addressParts = parts[0].Split(':');
            if (addressParts.Length != 2)
            {
                _logger35.AppendLine($"Can't parse address: {parts[0]}");
                return null;
            }

            int tcpPort;
            if (!int.TryParse(addressParts[1], out tcpPort))
            {
                _logger35.AppendLine($"Can't parse tcp port: {addressParts[1]}");
                return null;
            }

            var netAddress = new NetAddress(addressParts[0], tcpPort);
            if (!netAddress.HasValidIp4Address() || !netAddress.HasValidTcpPort())
            {
                _logger35.AppendLine($"Invalid ip address: {parts[0]}");
                return null;
            }

            return new ExtendedPort(netAddress, opticalPort);
        }

        public void RunMonitoringCycle()
        {
            while (true)
            {
                _measurementNumber++;

                var extendedPort = _monitoringQueue.Dequeue();
                _monitoringQueue.Enqueue(extendedPort);

                _logger35.AppendLine($"Measurement {_measurementNumber}  Port {extendedPort.ToStringA()} ...");
                ProcessOnePort(extendedPort);
                _logger35.AppendLine("Measurement is finished");

                if (_iniFile35.Read(IniSection.Monitoring, IniKey.IsMonitoringOn, 0) == 0)
                    break;
            }
        }

        private void ProcessOnePort(ExtendedPort extendedPort)
        {
            if (extendedPort.State == PortMeasResult.Ok || extendedPort.State == PortMeasResult.Unknown)
            {
                // FAST 
                var fastMoniResult = DoMeasurement(extendedPort, BaseRefType.Fast);
                if (GetPortState(fastMoniResult) != extendedPort.State ||
                    (extendedPort.LastFastSavedTimestamp - DateTime.Now) > _fastSaveTimespan)
                {
                    SendMoniResult(fastMoniResult);
                    extendedPort.LastFastSavedTimestamp = DateTime.Now;
                    extendedPort.State = GetPortState(fastMoniResult);
                    if (extendedPort.State == PortMeasResult.BrokenByFast)
                        extendedPort.IsBreakdownCloserThen20Km = fastMoniResult.FirstBreakDistance < 20;
                }
            }

            var isTraceBroken = extendedPort.State == PortMeasResult.BrokenByFast ||
                                extendedPort.State == PortMeasResult.BrokenByPrecise;
            var isSecondMeasurementNeeded = isTraceBroken ||
                (extendedPort.LastPreciseMadeTimestamp - DateTime.Now) > _preciseMakeTimespan;

            if (!isSecondMeasurementNeeded)
                return;

            // PRECISE (or ADDITIONAL)
            MoniResult moniResult;
            if (isTraceBroken && extendedPort.IsBreakdownCloserThen20Km && extendedPort.HasAdditionalBase())
                moniResult = DoMeasurement(extendedPort, BaseRefType.Additional);
            else
                moniResult = DoMeasurement(extendedPort, BaseRefType.Precise);
            extendedPort.LastPreciseMadeTimestamp = DateTime.Now;
            if (GetPortState(moniResult) != extendedPort.State ||
                (extendedPort.LastPreciseSavedTimestamp - DateTime.Now) > _preciseSaveTimespan)
            {
                SendMoniResult(moniResult);
                extendedPort.LastPreciseSavedTimestamp = DateTime.Now;
                extendedPort.State = GetPortState(moniResult);
            }
        }

        private void SendMoniResult(MoniResult moniResult)
        {

        }

        private PortMeasResult GetPortState(MoniResult moniResult)
        {
            if (!moniResult.IsFailed)
                return PortMeasResult.Ok;

            return moniResult.BaseRefType == BaseRefType.Fast
                ? PortMeasResult.BrokenByFast
                : PortMeasResult.BrokenByPrecise;
        }

        private MoniResult DoMeasurement(ExtendedPort extendedPort, BaseRefType baseRefType)
        {
            var baseBytes = GetBase(extendedPort, baseRefType);
            if (baseBytes == null)
                return null;
            _otdrManager.MeasureWithBase(baseBytes, _mainCharon.GetActiveChildCharon());
            return _otdrManager.CompareMeasureWithBase(baseBytes,
                _otdrManager.ApplyAutoAnalysis(_otdrManager.GetLastSorDataBuffer()), true); // is ApplyAutoAnalysis necessary ?
        }

        private byte[] GetBase(ExtendedPort extendedPort, BaseRefType baseRefType)
        {
            var basefile = $@"..\PortData\{extendedPort.GetFolderName()}\{baseRefType.ToFileName()}";
            if (File.Exists(basefile))
                return File.ReadAllBytes(basefile);
            _logger35.AppendLine($"Can't find {baseRefType.ToFileName()} for port {extendedPort.ToStringA()}");
            return null;
        }

    }
}