using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Dto;
using Iit.Fibertest.DirectCharonLibrary;
using Iit.Fibertest.IitOtdrLibrary;
using Iit.Fibertest.Utils35;
using WcfConnections;

namespace RtuManagement
{
    public partial class RtuManager
    {
        private const string DefaultIp = "192.168.88.101";

        private Guid _id;
        private readonly Logger35 _rtuLog;
        private readonly IniFile _rtuIni;
        private readonly Logger35 _serviceLog;
        private readonly IniFile _serviceIni;
        private OtdrManager _otdrManager;
        private Charon _mainCharon;

        public object WcfParameter { get; set; }

        private object _obj = new object();

        private readonly object _isMonitoringOnLocker = new object();
        private bool _isMonitoringOn;
        public bool IsMonitoringOn
        {
            get
            {
                lock (_isMonitoringOnLocker)
                {
                    return _isMonitoringOn;
                }
            }
            set
            {
                lock (_isMonitoringOnLocker)
                {
                    _isMonitoringOn = value;
                }
            }
        }

        private readonly object _isRtuInitializedLocker = new object();
        private bool _isRtuInitialized;
        public bool IsRtuInitialized
        {
            get
            {
                lock (_isRtuInitializedLocker)
                {
                    return _isRtuInitialized;
                }
            }
            set
            {
                lock (_isRtuInitializedLocker)
                {
                    _isRtuInitialized = value;
                }
            }
        }

        private bool _isMonitoringCancelled;

        public RtuManager(Logger35 serviceLog, IniFile serviceIni)
        {
            IsRtuInitialized = false;

            _serviceLog = serviceLog;
            _serviceIni = serviceIni;
            _serverIp = _serviceIni.Read(IniSection.DataCenter, IniKey.ServerIp, "192.168.96.179");

            _rtuIni = new IniFile();
            _rtuIni.AssignFile("RtuManager.ini");
            var cultureString = _rtuIni.Read(IniSection.General, IniKey.Culture, "ru-RU");

            _rtuLog = new Logger35();
            _rtuLog.AssignFile("RtuManager.log", cultureString);

            _mikrotikRebootTimeout =
                TimeSpan.FromSeconds(_rtuIni.Read(IniSection.Recovering, IniKey.MikrotikRebootTimeout, 40));
            _id = Guid.Parse(_rtuIni.Read(IniSection.DataCenter, IniKey.RtuGuid, Guid.Empty.ToString()));
        }

        /// <summary>
        /// user sends param when wants to initialize rtu
        /// </summary>
        /// <param name="param"></param>
        public void Initialize(object param = null)
        {
            _rtuLog.EmptyLine();
            _rtuLog.EmptyLine('-');

            IsRtuInitialized = false;
            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;
            _rtuLog.AppendLine($"RTU Manager started. Process {pid}, thread {tid}");

            bool isUserAskedInitialization = param != null;
            var rtu = param as InitializeRtuDto;
            if (rtu != null)
            {
                _rtuIni.Write(IniSection.DataCenter, IniKey.ServerIp, rtu.DataCenterIpAddress);
                _serverIp = rtu.DataCenterIpAddress;
                _rtuIni.Write(IniSection.DataCenter, IniKey.RtuGuid, rtu.RtuId.ToString());
            }
            RestoreFunctions.ResetCharonThroughComPort(_rtuIni, _rtuLog);

            if (InitializeRtuManager() != CharonOperationResult.Ok)
            {
                _rtuLog.AppendLine("Rtu Manager initialization failed.");
                if (rtu != null)
                    new R2DWcfManager(_serverIp, _serviceIni, _serviceLog).
                                SendInitializationConfirm(new RtuInitializedDto() { RtuId = rtu.RtuId, IsInitialized = false });
                return;
            }
            if (isUserAskedInitialization)
                new R2DWcfManager(_serverIp, _serviceIni, _serviceLog).SendInitializationConfirm(new RtuInitializedDto()
                {
                    RtuId = _id,
                    IsInitialized = true,
                    Serial = _mainCharon.Serial,
                    FullPortCount = _mainCharon.FullPortCount,
                    OwnPortCount = _mainCharon.OwnPortCount
                });
            IsRtuInitialized = true;

            IsMonitoringOn = _rtuIni.Read(IniSection.Monitoring, IniKey.IsMonitoringOn, 0) != 0;
            if (IsMonitoringOn)
                RunMonitoringCycle();
            else
            {
                var otdrAddress = _rtuIni.Read(IniSection.General, IniKey.OtdrIp, DefaultIp);
                _otdrManager.DisconnectOtdr(otdrAddress);
                _rtuLog.AppendLine("Rtu is in MANUAL mode.");
            }

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

            RunMonitoringCycle();

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
            lock (_obj)
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
                    lock (_obj)
                    {
                        _hasNewSettings = true;
                    }
                }
                else // should become MANUAL
                {
                    _otdrManager.InterruptMeasurement();
                    lock (_obj)
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

            new R2DWcfManager(_serverIp, _serviceIni, _serviceLog).SendMonitoringSettingsApplied(new MonitoringSettingsAppliedDto() {IsSuccessful = true});

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

            new R2DWcfManager(_serverIp, _serviceIni, _serviceLog).SendBaseRefAssigned(
                new BaseRefAssignedDto() {RtuId = assignBaseRefDto.RtuId, OtauPortDto = assignBaseRefDto.OtauPortDto, IsSuccessful = true});
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