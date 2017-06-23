using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Iit.Fibertest.DirectCharonLibrary;
using Iit.Fibertest.IitOtdrLibrary;
using Iit.Fibertest.Utils35;

namespace RtuManagement
{
    public class RtuManager
    {
        private const string DefaultIp = "192.168.88.101";

        private readonly Logger35 _rtuLog;
        private readonly IniFile _rtuIni;
        private OtdrManager _otdrManager;
        private Charon _mainCharon;

        private object _obj = new object();
        public bool IsMonitoringOn { get; set; }
        private bool _isMonitoringCancelled;
        private Queue<ExtendedPort> _monitoringQueue;
        private int _measurementNumber;
        private TimeSpan _preciseMakeTimespan;
        private TimeSpan _preciseSaveTimespan;
        private TimeSpan _fastSaveTimespan;

        public RtuManager()
        {
            _rtuIni = new IniFile();
            _rtuIni.AssignFile("RtuManager.ini");
            var cultureString = _rtuIni.Read(IniSection.General, IniKey.Culture, "ru-RU");

            _rtuLog = new Logger35();
            _rtuLog.AssignFile("RtuManager.log", cultureString);
            _rtuLog.EmptyLine();
            _rtuLog.EmptyLine('-');
        }

        public void Initialize()
        {
            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;
            _rtuLog.AppendLine($"RTU Manager started. Process {pid}, thread {tid}");

            RestoreFunctions.ResetCharonThroughComPort(_rtuIni, _rtuLog);
            IsMonitoringOn = _rtuIni.Read(IniSection.Monitoring, IniKey.IsMonitoringOn, 0) != 0;
            InitializeRtu();
            if (IsMonitoringOn)
                DoMonitoring();
            else
            {
                var otdrAddress = _rtuIni.Read(IniSection.General, IniKey.OtdrIp, DefaultIp);
                _otdrManager.DisconnectOtdr(otdrAddress);
                _rtuLog.AppendLine("Rtu is in MANUAL mode.");
            }
        }

        public void StartMonitoring()
        {
            if (IsMonitoringOn)
            {
                _rtuLog.AppendLine("Rtu is in AUTOMATIC mode already.");
                return;
            }

            _rtuLog.EmptyLine();
            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;
            _rtuLog.AppendLine($"Rtu is turned into AUTOMATIC mode. Process {pid}, thread {tid}");
            _rtuIni.Write(IniSection.Monitoring, IniKey.IsMonitoringOn, 1);
            InitializeRtu();
            DoMonitoring();
        }

        public void StopMonitoring()
        {
            if (!IsMonitoringOn)
            {
                _rtuLog.AppendLine("Rtu is in MANUAL mode already.");
                return;
            }

            lock (_obj)
            {
                _otdrManager.InterruptMeasurement();
                _isMonitoringCancelled = true;
                _rtuIni.Write(IniSection.Monitoring, IniKey.IsMonitoringOn, 0);
            }
            Thread.Sleep(TimeSpan.FromMilliseconds(2000));
            var otdrAddress = _rtuIni.Read(IniSection.General, IniKey.OtdrIp, DefaultIp);
            _otdrManager.DisconnectOtdr(otdrAddress);
            _rtuLog.AppendLine("Rtu is turned into MANUAL mode.");
            lock (_obj)
            {
                IsMonitoringOn = false;
                _isMonitoringCancelled = false;
            }
        }

        private bool InitializeRtu()
        {
            LedDisplay.Show(_rtuIni, _rtuLog, LedDisplayCode.Wait);
            if (!InitializeOtdr())
            {
                _rtuLog.AppendLine("Otdr initialization failed.");
                return false;
            }

            if (!InitializeOtau())
            {
                _rtuLog.AppendLine("Otau initialization failed.");
                return false;
            }
            return true;
        }

        private void DoMonitoring()
        {
            _rtuIni.Write(IniSection.Monitoring, IniKey.IsMonitoringOn, 1);
            GetMonitoringQueue();
            GetMonitoringParams();

            RunMonitoringCycle();

            _rtuLog.AppendLine("Monitoring stopped.");
        }

        private void RestoreOtdrConnection()
        {
            var arp = _rtuIni.Read(IniSection.Restore, IniKey.ClearArp, 0);
            if (arp != 0)
            {
                _rtuIni.Write(IniSection.Restore, IniKey.ClearArp, 0);
                ClearArp();
            }

            var reboot = _rtuIni.Read(IniSection.Restore, IniKey.RebootSystem, 0);
            if (reboot != 0)
            {
                _rtuIni.Write(IniSection.Restore, IniKey.RebootSystem, 0);
                RestoreFunctions.RebootSystem(_rtuLog);
            }
        }

        private bool InitializeOtdr()
        {
            _otdrManager = new OtdrManager(@"OtdrMeasEngine\", _rtuIni, _rtuLog);
            if (_otdrManager.LoadDll() != "")
                return false;
            
            var otdrAddress = _rtuIni.Read(IniSection.General, IniKey.OtdrIp, DefaultIp);
            if (_otdrManager.InitializeLibrary())
                _otdrManager.ConnectOtdr(otdrAddress);
            return _otdrManager.IsOtdrConnected;
        }

        public bool InitializeOtau()
        {
            var otauIpAddress = _rtuIni.Read(IniSection.General, IniKey.OtauIp, DefaultIp);
            _mainCharon = new Charon(new NetAddress(otauIpAddress, 23), _rtuIni, _rtuLog);
            return _mainCharon.Initialize();
        }

        public void GetMonitoringQueue()
        {
            _rtuLog.EmptyLine();
            _rtuLog.AppendLine("Monitoring queue assembling...");
            _monitoringQueue = new Queue<ExtendedPort>();
            var monitoringSettingsFile = Utils.FileNameForSure(@"..\ini\", @"monitoring.que", false);
            var content = File.ReadAllLines(monitoringSettingsFile);
            foreach (var line in content)
            {
                var extendedPort = Create(line);
                if (extendedPort != null && _mainCharon.IsExtendedPortValidForMonitoring(extendedPort))
                    _monitoringQueue.Enqueue(extendedPort);
            }
            _rtuLog.AppendLine($"{_monitoringQueue.Count} port(s) in queue.");
        }

        public void GetMonitoringParams()
        {
            _preciseMakeTimespan =
                TimeSpan.FromSeconds(_rtuIni.Read(IniSection.Monitoring, IniKey.PreciseMakeTimespan, 3600));
            _preciseSaveTimespan =
                TimeSpan.FromSeconds(_rtuIni.Read(IniSection.Monitoring, IniKey.PreciseSaveTimespan, 3600));
            _fastSaveTimespan =
                TimeSpan.FromSeconds(_rtuIni.Read(IniSection.Monitoring, IniKey.FastSaveTimespan, 3600));
        }

        private ExtendedPort Create(string str)
        {
            if (string.IsNullOrEmpty(str))
                return null;

            var parts = str.Split('-');
            if (parts.Length != 2)
            {
                _rtuLog.AppendLine($"Invalid string in queue file: '{str}'", 2);
                return null;
            }

            int opticalPort;
            if (!int.TryParse(parts[1], out opticalPort))
            {
                _rtuLog.AppendLine($"Can't parse optical port: '{parts[1]}'", 2);
                return null;
            }

            var addressParts = parts[0].Split(':');
            if (addressParts.Length != 2)
            {
                _rtuLog.AppendLine($"Can't parse address: '{parts[0]}'", 2);
                return null;
            }

            int tcpPort;
            if (!int.TryParse(addressParts[1], out tcpPort))
            {
                _rtuLog.AppendLine($"Can't parse tcp port: '{addressParts[1]}'", 2);
                return null;
            }

            var netAddress = new NetAddress(addressParts[0], tcpPort);
            if (!netAddress.HasValidIp4Address() || !netAddress.HasValidTcpPort())
            {
                _rtuLog.AppendLine($"Invalid ip address: '{parts[0]}'", 2);
                return null;
            }

            return new ExtendedPort(netAddress, opticalPort);
        }

        public void RunMonitoringCycle()
        {
            _rtuLog.EmptyLine();
            _rtuLog.AppendLine("Start monitoring.");
            IsMonitoringOn = _rtuIni.Read(IniSection.Monitoring, IniKey.IsMonitoringOn, 0) != 0;
            if (!IsMonitoringOn)
                _rtuLog.AppendLine("Monitoring is off");

            if (_monitoringQueue.Count < 1)
            {
                _rtuLog.AppendLine("There are no ports in queue for monitoring.");
                _rtuIni.Write(IniSection.Monitoring, IniKey.IsMonitoringOn, 0);
                IsMonitoringOn = false;
                return;
            }
            while (true)
            {
                _measurementNumber++;

                var extendedPort = _monitoringQueue.Dequeue();
                _monitoringQueue.Enqueue(extendedPort);

                _rtuLog.EmptyLine();
                ProcessOnePort(extendedPort);

                lock (_obj)
                {
                    if (_isMonitoringCancelled)
                        break;
                }
            }
        }

        private void ProcessOnePort(ExtendedPort extendedPort)
        {
            var hasFastPerformed = false;
            if (extendedPort.State == PortMeasResult.Ok || extendedPort.State == PortMeasResult.Unknown)
            {
                // FAST 
                _rtuLog.AppendLine($"MEAS. {_measurementNumber} port {_mainCharon.GetBopPortString(extendedPort)}, Fast");
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
                _rtuLog.EmptyLine();

            // PRECISE (or ADDITIONAL)
            var baseType = (isTraceBroken && extendedPort.IsBreakdownCloserThen20Km && extendedPort.HasAdditionalBase())
                ? BaseRefType.Additional
                : BaseRefType.Precise;
            var message = $"MEAS. {_measurementNumber} port {_mainCharon.GetBopPortString(extendedPort)}, {baseType}";
            message += hasFastPerformed ? " (confirmation)" : "";
            _rtuLog.AppendLine(message);
            var moniResult = DoMeasurement(baseType, extendedPort, !hasFastPerformed);
            if (moniResult == null)
                return;
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
            _rtuLog.AppendLine($"Sending monitoring result {moniResult.BaseRefType} to server...");

        }

        // only is trace OK or not, without details of breakdown if any
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
            if (_isMonitoringCancelled)
            {
                return null;
            }
            var measBytes = _otdrManager.ApplyAutoAnalysis(_otdrManager.GetLastSorDataBuffer()); // is ApplyAutoAnalysis necessary ?
            var moniResult = _otdrManager.CompareMeasureWithBase(baseBytes, ref measBytes, true); // base is inserted into meas during comparison
            SaveMeas(extendedPort, baseRefType, measBytes); // so save after comparison
            return moniResult; 
        }

        private byte[] GetBase(ExtendedPort extendedPort, BaseRefType baseRefType)
        {
            var appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var appDir = Path.GetDirectoryName(appPath);
            var basefile = appDir + $@"\..\PortData\{extendedPort.GetFolderName()}\{baseRefType.ToBaseFileName()}";
            if (File.Exists(basefile))
                return File.ReadAllBytes(basefile);
            _rtuLog.AppendLine($"Can't find {basefile}");
            return null;
        }

        private void SaveMeas(ExtendedPort extendedPort, BaseRefType baseRefType, byte[] bytes)
        {
            var appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var appDir = Path.GetDirectoryName(appPath);
            var measfile = appDir + $@"\..\PortData\{extendedPort.GetFolderName()}\{baseRefType.ToMeasFileName()}";
            File.WriteAllBytes(measfile, bytes);
        }

        public void ClearArp()
        {
            var res = Arp.GetTable();
            _rtuLog.AppendLine(res);
            res = Arp.ClearCache();
            _rtuLog.AppendLine($"Clear ARP table - {res}");
            res = Arp.GetTable();
            _rtuLog.AppendLine(res);
        }


        /* 
        private static void SendMoniResult(MoniResult moniResult)
        {
//                            var queueName = @".\private$\F20";
            var queueName = @"FormatName:Direct=TCP:192.168.96.8\private$\F20";
            //                var queueName = @"FormatName:Direct=TCP:192.168.96.52\private$\F22";
            //                var queueName = @"FormatName:Direct=OS:opx-lmarholin\private$\F22";

            // if (!MessageQueue.Exists(queueName)) // works only for local machine queue/address format
            //     break;

            using (MessageQueue queue = new MessageQueue(queueName))
            {
                var binaryMessageFormatter = new BinaryMessageFormatter();
                using (var message = new Message(moniResult, binaryMessageFormatter))
                {
                    message.Recoverable = true;
                    queue.Send(message, MessageQueueTransactionType.Single);
//                    queue.Send(message);
                }
            }

        }
        */
    }
}