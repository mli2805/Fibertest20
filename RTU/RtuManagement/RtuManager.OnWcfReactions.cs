using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
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
                SendToUserInitializationResult(initializationResult);
            if (initializationResult != CharonOperationResult.Ok)
                return;
            IsRtuInitialized = true;

            var dove = new Dove(_serverAddresses, _serviceIni, _serviceLog) {QueueOfMoniResultsOnDisk = QueueOfMoniResultsOnDisk};
            var thread = new Thread(dove.Fly) {IsBackground = true};
            thread.Start();

            IsMonitoringOn = _rtuIni.Read(IniSection.Monitoring, IniKey.IsMonitoringOn, 0) != 0;
            if (IsMonitoringOn)
                RunMonitoringCycle(isUserAskedInitialization);
            else
                DisconnectOtdr();
        }

        private void SaveInitializationParameters(InitializeRtuDto rtu)
        {
            _rtuIni.Write(IniSection.Server, IniKey.RtuGuid, rtu.RtuId.ToString());

            _rtuIni.WriteServerAddresses(rtu.ServerAddresses);
            _serverAddresses = rtu.ServerAddresses;
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

        private void SaveNewQueueToFile(ApplyMonitoringSettingsDto dto)
        {
            var otdrIp = _rtuIni.Read(IniSection.General, IniKey.OtdrIp, "192.168.88.101");
            var content = dto.Ports.Select(port => port.IsPortOnMainCharon
                    ? $"{otdrIp}:{port.TcpPort}-{port.OpticalPort}"
                    : $"{port.Ip}:{port.TcpPort}-{port.OpticalPort}")
                .ToList();

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
                ? $@"{otdrIp}t{assignBaseRefDto.OtauPortDto.TcpPort}p{assignBaseRefDto.OtauPortDto.OpticalPort}\"
                : $@"{assignBaseRefDto.OtauPortDto.Ip}t{assignBaseRefDto.OtauPortDto.TcpPort}p{assignBaseRefDto.OtauPortDto.OpticalPort}\";

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
            if (port.TcpPort == 23)
                port.Ip = _rtuIni.Read(IniSection.General, IniKey.OtdrIp, "192.168.88.101");
            var toggleResult = _mainCharon.SetExtendedActivePort(new NetAddress(port.Ip, port.TcpPort), port.OpticalPort);

            return toggleResult == CharonOperationResult.Ok;
        }
    }
}
