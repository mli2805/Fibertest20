using System;
using System.Collections.Generic;
using System.IO;
using Iit.Fibertest.DirectCharonLibrary;
using Iit.Fibertest.IitOtdrLibrary;
using Iit.Fibertest.Utils35;

namespace ConsoleAppOtdr
{
    public class RtuManager
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

        public RtuManager(Logger35 logger35, IniFile iniFile35)
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
            _logger35.EmptyLine();
            _logger35.AppendLine("Monitoring queue assembling...");
            _monitoringQueue = new Queue<ExtendedPort>();
            var monitoringSettingsFile = Utils.FileNameForSure(@"..\ini\", @"monitoring.que", false);
            var content = File.ReadAllLines(monitoringSettingsFile);
            foreach (var line in content)
            {
                var extendedPort = Create(line);
                if (extendedPort != null && _mainCharon.IsExtendedPortValidForMonitoring(extendedPort))
                    _monitoringQueue.Enqueue(extendedPort);
            }
            _logger35.AppendLine($"{_monitoringQueue.Count} port(s) in queue.");
        }

        public void GetMonitoringParams()
        {
            _preciseMakeTimespan =
                TimeSpan.FromSeconds(_iniFile35.Read(IniSection.Monitoring, IniKey.PreciseMakeTimespan, 3600));
            _preciseSaveTimespan =
                TimeSpan.FromSeconds(_iniFile35.Read(IniSection.Monitoring, IniKey.PreciseSaveTimespan, 3600));
            _fastSaveTimespan =
                TimeSpan.FromSeconds(_iniFile35.Read(IniSection.Monitoring, IniKey.FastSaveTimespan, 3600));
        }

        private ExtendedPort Create(string str)
        {
            if (string.IsNullOrEmpty(str))
                return null;

            var parts = str.Split('-');
            if (parts.Length != 2)
            {
                _logger35.AppendLine($"Invalid string in queue file: '{str}'", 2);
                return null;
            }

            int opticalPort;
            if (!int.TryParse(parts[1], out opticalPort))
            {
                _logger35.AppendLine($"Can't parse optical port: '{parts[1]}'", 2);
                return null;
            }

            var addressParts = parts[0].Split(':');
            if (addressParts.Length != 2)
            {
                _logger35.AppendLine($"Can't parse address: '{parts[0]}'", 2);
                return null;
            }

            int tcpPort;
            if (!int.TryParse(addressParts[1], out tcpPort))
            {
                _logger35.AppendLine($"Can't parse tcp port: '{addressParts[1]}'", 2);
                return null;
            }

            var netAddress = new NetAddress(addressParts[0], tcpPort);
            if (!netAddress.HasValidIp4Address() || !netAddress.HasValidTcpPort())
            {
                _logger35.AppendLine($"Invalid ip address: '{parts[0]}'", 2);
                return null;
            }

            return new ExtendedPort(netAddress, opticalPort);
        }

        public void RunMonitoringCycle()
        {
            _logger35.EmptyLine();
            _logger35.AppendLine("Start monitoring.");
            if (_monitoringQueue.Count < 1)
            {
                _logger35.AppendLine("There are no ports in queue for monitoring.");
                return;
            }
            while (true)
            {
                _measurementNumber++;

                var extendedPort = _monitoringQueue.Dequeue();
                _monitoringQueue.Enqueue(extendedPort);

                _logger35.EmptyLine();
                ProcessOnePort(extendedPort);

                if (_iniFile35.Read(IniSection.Monitoring, IniKey.IsMonitoringOn, 0) == 0)
                    break;
            }
        }

        private void ProcessOnePort(ExtendedPort extendedPort)
        {
            var hasFastPerformed = false;
            if (extendedPort.State == PortMeasResult.Ok || extendedPort.State == PortMeasResult.Unknown)
            {
                // FAST 
                _logger35.AppendLine($"MEAS. {_measurementNumber} port {_mainCharon.GetBopPortString(extendedPort)}, Fast");
                var fastMoniResult = DoMeasurement(BaseRefType.Fast, extendedPort);
                if (fastMoniResult == null)
                    return;
                hasFastPerformed = true;
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
                                            (DateTime.Now - extendedPort.LastPreciseMadeTimestamp) >
                                            _preciseMakeTimespan;

            if (!isSecondMeasurementNeeded)
                return;
            if (hasFastPerformed)
                _logger35.EmptyLine();

            // PRECISE (or ADDITIONAL)
            var baseType = (isTraceBroken && extendedPort.IsBreakdownCloserThen20Km && extendedPort.HasAdditionalBase())
                ? BaseRefType.Additional
                : BaseRefType.Precise;
            var message = $"MEAS. {_measurementNumber} port {_mainCharon.GetBopPortString(extendedPort)}, {baseType}";
            message += hasFastPerformed ? " (confirmation)" : "";
            _logger35.AppendLine(message);
            var moniResult = DoMeasurement(baseType, extendedPort, !hasFastPerformed);
            extendedPort.LastPreciseMadeTimestamp = DateTime.Now;
            if (GetPortState(moniResult) != extendedPort.State ||
                (DateTime.Now - extendedPort.LastPreciseSavedTimestamp) > _preciseSaveTimespan)
            {
                SendMoniResult(moniResult);
                extendedPort.LastPreciseSavedTimestamp = DateTime.Now;
                extendedPort.State = GetPortState(moniResult);
            }
        }

        private void SendMoniResult(MoniResult moniResult)
        {
            _logger35.AppendLine("Sending monitoring result to server...");
        }

        // only is trace OK or not, without character of breakdown
        private PortMeasResult GetPortState(MoniResult moniResult)
        {
            if (!moniResult.IsFailed && !moniResult.IsFiberBreak && !moniResult.IsNoFiber)
                return PortMeasResult.Ok;

            return moniResult.BaseRefType == BaseRefType.Fast
                ? PortMeasResult.BrokenByFast
                : PortMeasResult.BrokenByPrecise;
        }

        private MoniResult DoMeasurement(BaseRefType baseRefType, ExtendedPort extendedPort, bool isPortChanged = true)
        {
            if (isPortChanged && !_mainCharon.SetExtendedActivePort(extendedPort.NetAddress, extendedPort.Port))
                return null;
            var baseBytes = GetBase(extendedPort, baseRefType);
            if (baseBytes == null)
                return null;
            if (!_otdrManager.MeasureWithBase(baseBytes, _mainCharon.GetActiveChildCharon()))
                return null;
            var measBytes = _otdrManager.ApplyAutoAnalysis(_otdrManager.GetLastSorDataBuffer()); // is ApplyAutoAnalysis necessary ?
            var moniResult = _otdrManager.CompareMeasureWithBase(baseBytes, measBytes, true); // base is inserted into meas during comparison
            SaveMeas(extendedPort, baseRefType, measBytes); // so save after comparison
            return moniResult; 
        }

        private byte[] GetBase(ExtendedPort extendedPort, BaseRefType baseRefType)
        {
            var basefile = $@"..\PortData\{extendedPort.GetFolderName()}\{baseRefType.ToBaseFileName()}";
            if (File.Exists(basefile))
                return File.ReadAllBytes(basefile);
            _logger35.AppendLine($"Can't find {baseRefType.ToBaseFileName()} for port {extendedPort.ToStringA()}");
            return null;
        }

        private void SaveMeas(ExtendedPort extendedPort, BaseRefType baseRefType, byte[] bytes)
        {
            var measfile = $@"..\PortData\{extendedPort.GetFolderName()}\{baseRefType.ToMeasFileName()}";
            File.WriteAllBytes(measfile, bytes);
        }


    }
}