using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Iit.Fibertest.DirectCharonLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.RtuManagement
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
            if (initializationResult != CharonOperationResult.Ok)
                return;
            IsRtuInitialized = true;

            if (_woodPecker != null)
                _woodPecker.IsCancelled = true;
            _woodPecker = new WoodPecker(_id, _version, _serverAddresses, _serviceIni, _serviceLog);
            var woodpeckerThread = new Thread(_woodPecker.Start) {IsBackground = true};
            woodpeckerThread.Start();

//            if (_dove != null)
//                _dove.IsCancelled = true;
//            _dove = new Dove(_serverAddresses, _serviceIni, _serviceLog) {QueueOfMoniResultsOnDisk = QueueOfMoniResultsOnDisk};
//            var doveThread = new Thread(_dove.Start) {IsBackground = true};
//            doveThread.Start();

            IsMonitoringOn = _rtuIni.Read(IniSection.Monitoring, IniKey.IsMonitoringOn, 0) != 0;
            if (IsMonitoringOn)
                RunMonitoringCycle();
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

//            _otdrManager.InterruptMeasurement();
           _cts.Cancel();
        }

        public void ChangeSettings(ApplyMonitoringSettingsDto settings)
        {
            
        }

        private void ApplyChangeSettings()
        {
            _rtuLog.EmptyLine();
            _rtuLog.AppendLine("Start ApplyChangeSettings");
            var dto = WcfParameter as ApplyMonitoringSettingsDto;
            if (dto == null)
                return;

            ApplyNewFrequenciesToIni(dto.Timespans);
            _monitoringQueue.MergeNewPortsIntQueue(dto.Ports);
            _rtuLog.AppendLine($"Queue merged. {_monitoringQueue.Count()} port(s) in queue");
            _monitoringQueue.Save();

            if (!_hasNewSettings &&  // in MANUAL mode so far
                    dto.IsMonitoringOn)
                        StartMonitoring();
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
        }

        private void ApplyNewFrequenciesToIni(MonitoringTimespansDto dto)
        {
            _preciseMakeTimespan = dto.PreciseMeas;
            _rtuIni.Write(IniSection.Monitoring, IniKey.PreciseMakeTimespan, (int)dto.PreciseMeas.TotalSeconds);
            _preciseSaveTimespan = dto.PreciseSave;
            _rtuIni.Write(IniSection.Monitoring, IniKey.PreciseSaveTimespan, (int)dto.PreciseSave.TotalSeconds);
            _fastSaveTimespan = dto.FastSave;
            _rtuIni.Write(IniSection.Monitoring, IniKey.FastSaveTimespan, (int)dto.FastSave.TotalSeconds);
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
