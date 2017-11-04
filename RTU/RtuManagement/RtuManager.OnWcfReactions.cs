using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Iit.Fibertest.DirectCharonLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.RtuManagement
{
    public partial class RtuManager
    {
        // could contain only one element at any time
        // used as thread safe way to exchange between Wcf thread and Measurement thread
        public ConcurrentQueue<object> WcfCommandsQueue = new ConcurrentQueue<object>();

        public void Initialize(InitializeRtuDto param, Action callback)
        {
            if (IsMonitoringOn)
            {
                IsMonitoringOn = false;
                _rtuLog.AppendLine("Interrupting current measurement...");
                _otdrManager.InterruptMeasurement();
                Thread.Sleep(TimeSpan.FromSeconds(5)); //for long measurements it could be not enough!!!
                _rtuIni.Write(IniSection.Monitoring, IniKey.IsMonitoringOn, 1);
            }

            LogInitializationStart();

            IsRtuInitialized = false;

            if (param != null)
                SaveInitializationParameters(param);
            RestoreFunctions.ResetCharonThroughComPort(_rtuIni, _rtuLog);

            _rtuInitializationResult = InitializeRtuManager();
            if (_rtuInitializationResult != ReturnCode.Ok)
            {
                // usualy can't find some file
                // in other cases if there is a way to recover it doesn't come here but start recover procedure
                callback?.Invoke();
                return;
            }

            IsRtuInitialized = true;
            callback?.Invoke();

            IsMonitoringOn = _rtuIni.Read(IniSection.Monitoring, IniKey.IsMonitoringOn, 0) != 0;
            if (IsMonitoringOn)
                RunMonitoringCycle();
            else
                DisconnectOtdr();
        }

        private void LogInitializationStart()
        {
            _rtuLog.EmptyLine();
            _rtuLog.EmptyLine('-');

            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;
            _rtuLog.AppendLine($"RTU initialization started. Process {pid}, thread {tid}");
        }

        private void SaveInitializationParameters(InitializeRtuDto rtu)
        {
            _id = rtu.RtuId;
            _serviceIni.Write(IniSection.Server, IniKey.RtuGuid, _id.ToString());

            _serverAddresses = (DoubleAddress)rtu.ServerAddresses.Clone();
            _serviceIni.WriteServerAddresses(_serverAddresses);
        }

        public void StartMonitoring(Action callback)
        {
            IsRtuInitialized = false;
            InitializeRtuManager();
            IsRtuInitialized = true;

            _rtuLog.EmptyLine();
            _rtuLog.AppendLine("Rtu is turned into AUTOMATIC mode.");
            _rtuIni.Write(IniSection.Monitoring, IniKey.IsMonitoringOn, 1);
            IsMonitoringOn = true;
            callback?.Invoke();

            RunMonitoringCycle();
        }

        public void StopMonitoring()
        {
            IsMonitoringOn = false;
           _otdrManager.InterruptMeasurement();
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
                StartMonitoring(null);
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
                StartMonitoring(null);
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
