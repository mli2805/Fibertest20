using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web.Script.Serialization;
using Dto;
using Iit.Fibertest.DirectCharonLibrary;
using Iit.Fibertest.UtilsLib;
using WcfConnections;

namespace RtuManagement
{
    public partial class RtuManager
    {
        /// <summary>
        /// user sends param when wants to initialize rtu
        /// elsewhere it is initialization on start
        /// </summary>
        /// <param name="param"></param>
        public void Initialize(object param = null)
        {
            _rtuLog.EmptyLine();
            _rtuLog.EmptyLine('-');

            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;
            _rtuLog.AppendLine($"RTU Manager started. Process {pid}, thread {tid}");

            IsRtuInitialized = false;

            bool isUserAskedInitialization = param != null;
            var rtu = param as InitializeRtuDto;
            if (rtu != null)
                SaveInitializationParameters(rtu);
            RestoreFunctions.ResetCharonThroughComPort(_rtuIni, _rtuLog);

            var initializationResult = InitializeRtuManager();
            if (isUserAskedInitialization)
                SendToUserInitializationResult(initializationResult, rtu?.RtuAddresses);
            if (initializationResult != CharonOperationResult.Ok)
                return;
            IsRtuInitialized = true;

            if (_woodPecker != null)
                _woodPecker.IsCancelled = true;
            _woodPecker = new WoodPecker(_id, _version, _serverAddresses, _serviceIni, _serviceLog);
            var woodpeckerThread = new Thread(_woodPecker.Start) {IsBackground = true};
            woodpeckerThread.Start();

            if (_dove != null)
                _dove.IsCancelled = true;
            _dove = new Dove(_serverAddresses, _serviceIni, _serviceLog) {QueueOfMoniResultsOnDisk = QueueOfMoniResultsOnDisk};
            var doveThread = new Thread(_dove.Start) {IsBackground = true};
            doveThread.Start();

            IsMonitoringOn = _rtuIni.Read(IniSection.Monitoring, IniKey.IsMonitoringOn, 0) != 0;
            if (IsMonitoringOn)
                RunMonitoringCycle(isUserAskedInitialization);
            else
                DisconnectOtdr();
        }

        private void SaveInitializationParameters(InitializeRtuDto rtu)
        {
            _id = rtu.RtuId;
            _serviceIni.Write(IniSection.Server, IniKey.RtuGuid, _id.ToString());

            _serverAddresses = new DoubleAddressWithConnectionStats() {DoubleAddress = (DoubleAddress)rtu.ServerAddresses.Clone()};
            _serviceIni.WriteServerAddresses(_serverAddresses.DoubleAddress);
        }

        public void StartMonitoring()
        {
            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;
            _rtuLog.EmptyLine();
            _rtuLog.AppendLine($"RtuManager now in process {pid}, thread {tid}");

            if (IsMonitoringOn)
            {
                _rtuLog.AppendLine("Rtu is in AUTOMATIC mode already.");
                return;
            }

            IsRtuInitialized = false;
            InitializeRtuManager();
            IsRtuInitialized = true;

            _rtuLog.EmptyLine();
            _rtuLog.AppendLine($"Rtu is turned into AUTOMATIC mode. Process {pid}, thread {tid}");
            _rtuIni.Write(IniSection.Monitoring, IniKey.IsMonitoringOn, 1);
            IsMonitoringOn = true;

            RunMonitoringCycle(true);
        }

        public void StopMonitoring()
        {
            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;
            _rtuLog.AppendLine($"RtuManager now in process {pid}, thread {tid}");

            if (!IsMonitoringOn)
            {
                _rtuLog.AppendLine("Rtu is in MANUAL mode already.");
                return;
            }

            _otdrManager.InterruptMeasurement();
            lock (_isMonitoringCancelledLocker)
            {
                _isMonitoringCancelled = true;
            }
        }

        public void ChangeSettings(ApplyMonitoringSettingsDto settings)
        {
            // only save variable and leave in order to not block wcf thread
            WcfParameter = settings;

            if (IsMonitoringOn) // AUTO
            {
                if (settings.IsMonitoringOn)
                {
                    lock (_isMonitoringCancelledLocker)
                    {
                        _hasNewSettings = true;
                    }
                }
                else // should become MANUAL
                {
                    _otdrManager.InterruptMeasurement();
                    lock (_isMonitoringCancelledLocker)
                    {
                        _isMonitoringCancelled = true;
                    }
                    var thread = new Thread(ApplyChangeSettings);
                    thread.Start();
                }
            }
            else  // MANUAL
            {
                var thread = new Thread(ApplyChangeSettings);
                thread.Start();
            }
        }

        private void ApplyChangeSettings()
        {
            _rtuLog.EmptyLine();
            _rtuLog.AppendLine("Start ApplyChangeSettings");
            var dto = WcfParameter as ApplyMonitoringSettingsDto;
            if (dto == null)
                return;

            SaveNewFrequenciesToIni(dto);
            SaveNewQueueToFile(dto);
            SerializePortsToQueue(dto.Ports);

            new R2DWcfManager(_serverAddresses, _serviceIni, _serviceLog).SendMonitoringSettingsApplied(new MonitoringSettingsAppliedDto() { IsSuccessful = true });

            if (_hasNewSettings) // in AUTOMATIC mode already
            {
                GetMonitoringQueue();
                GetMonitoringParams();
            }
            else // in MANUAL mode so far
            if (dto.IsMonitoringOn)
                StartMonitoring();
        }

        private Queue<ExtendedPort> MergeNewPortsWithQueue(List<OtauPortDto> ports)
        {
            var newQueue = new Queue<ExtendedPort>();

            foreach (var otauPortDto in ports)
            {
                var extendedPort = _monitoringQueue.FirstOrDefault(p=>p.IsTheSamePort(otauPortDto)) ?? new ExtendedPort(otauPortDto);
                newQueue.Enqueue(extendedPort);
            }

            return newQueue;
        }

        private void SerializePortsToQueue(List<OtauPortDto> ports)
        {
            var monitoringSettingsFile = Utils.FileNameForSure(@"..\ini\", @"monitoring.que", false);
            var javaScriptSerializer = new JavaScriptSerializer();
            var contents = ports.Select(p => javaScriptSerializer.Serialize(p)).ToList();
            File.WriteAllLines(monitoringSettingsFile,contents);
        }

        private void SaveNewQueueToFile(ApplyMonitoringSettingsDto dto)
        {
            var otdrIp = _rtuIni.Read(IniSection.General, IniKey.OtdrIp, "192.168.88.101");

            var content = new List<string>();
            foreach (var otauPortDto in dto.Ports)
            {
                var sameExtendedPort =
                    otauPortDto.IsPortOnMainCharon 
                    ? _monitoringQueue.FirstOrDefault(p => p.NetAddress.Ip4Address == otdrIp && p.OpticalPort == otauPortDto.OpticalPort)
                    : _monitoringQueue.FirstOrDefault(p => p.NetAddress.Ip4Address == otauPortDto.OtauIp && p.OpticalPort == otauPortDto.OpticalPort);
                var state = sameExtendedPort == null ? 2 : (int)sameExtendedPort.LastTraceState;

                content.Add(otauPortDto.IsPortOnMainCharon
                    ? $"{otdrIp}:{otauPortDto.OtauTcpPort}-{otauPortDto.OpticalPort} {state}"
                    : $"{otauPortDto.OtauIp}:{otauPortDto.OtauTcpPort}-{otauPortDto.OpticalPort} {state}");
            }
            
            var monitoringSettingsFile = Utils.FileNameForSure(@"..\ini\", @"monitoring.que", false);
            File.WriteAllLines(monitoringSettingsFile, content);
        }

        public void AssignBaseRefs(object param)
        {
            _rtuLog.AppendLine("Base refs received.");
            var assignBaseRefDto = param as AssignBaseRefDto;
            if (assignBaseRefDto == null)
                return;

            _rtuLog.AppendLine("Base refs valid dto.");
            var otdrIp = _rtuIni.Read(IniSection.General, IniKey.OtdrIp, "192.168.88.101");

            var portFolderName = assignBaseRefDto.OtauPortDto.IsPortOnMainCharon
                ? $@"{otdrIp}t{assignBaseRefDto.OtauPortDto.OtauTcpPort}p{assignBaseRefDto.OtauPortDto.OpticalPort}\"
                : $@"{assignBaseRefDto.OtauPortDto.OtauIp}t{assignBaseRefDto.OtauPortDto.OtauTcpPort}p{assignBaseRefDto.OtauPortDto.OpticalPort}\";

            var appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var appDir = Path.GetDirectoryName(appPath);
            if (appDir == null)
                return;
            var fullFolderName = Path.Combine(appDir, @"..\PortData\", portFolderName);
            if (!Directory.Exists(fullFolderName))
                Directory.CreateDirectory(fullFolderName);

            _rtuLog.AppendLine($"Base refs will be saved in {fullFolderName}.");
            foreach (var baseRef in assignBaseRefDto.BaseRefs)
            {
                var filename = baseRef.BaseRefType.ToBaseFileName();

                var fullPath = Path.Combine(fullFolderName, filename);
                _rtuLog.AppendLine($"with name: {fullPath}");

                if (File.Exists(fullPath))
                    File.Delete(fullPath);
                if (baseRef.SorBytes != null)
                    File.WriteAllBytes(fullPath, baseRef.SorBytes);
            }

            new R2DWcfManager(_serverAddresses, _serviceIni, _serviceLog).SendBaseRefAssigned(
                new BaseRefAssignedDto() { RtuId = assignBaseRefDto.RtuId, OtauPortDto = assignBaseRefDto.OtauPortDto, IsSuccessful = true });
        }

        private void SaveNewFrequenciesToIni(ApplyMonitoringSettingsDto dto)
        {
            _rtuIni.Write(IniSection.Monitoring, IniKey.PreciseMakeTimespan, (int)dto.Timespans.PreciseMeas.TotalSeconds);
            _rtuIni.Write(IniSection.Monitoring, IniKey.PreciseSaveTimespan, (int)dto.Timespans.PreciseSave.TotalSeconds);
            _rtuIni.Write(IniSection.Monitoring, IniKey.FastSaveTimespan, (int)dto.Timespans.FastSave.TotalSeconds);
        }

        public void MeasurementOutOfTurn()
        {
            var wasMonitoringOn = IsMonitoringOn;
            if (wasMonitoringOn)
            {
                StopMonitoring();
                Thread.Sleep(TimeSpan.FromSeconds(3));
            }

            // TODO measurement

            if (wasMonitoringOn)
                StartMonitoring();
        }

        public bool ToggleToPort(OtauPortDto port)
        {
            if (port.OtauTcpPort == 23)
                port.OtauIp = _rtuIni.Read(IniSection.General, IniKey.OtdrIp, "192.168.88.101");
            var toggleResult = _mainCharon.SetExtendedActivePort(new NetAddress(port.OtauIp, port.OtauTcpPort), port.OpticalPort);

            return toggleResult == CharonOperationResult.Ok;
        }
    }
}
