using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using Iit.Fibertest.DirectCharonLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.RtuManagement
{
    public partial class RtuManager
    {
        // could contain only one element at any time
        // used as thread safe way to exchange between WCF thread and Measurement thread
        public ConcurrentQueue<object> WcfCommandsQueue = new ConcurrentQueue<object>();

        public ConcurrentQueue<object> ShouldSendHeartbeat = new ConcurrentQueue<object>();

        public bool Initialize(InitializeRtuDto param, Action callback)
        {
            // prohibit to send heartbeats
         //   ShouldSendHeartbeat.TryDequeue(out _);

            if (IsMonitoringOn || _wasMonitoringOn)
            {
                StopMonitoring("Initialize");
                _rtuIni.Write(IniSection.Monitoring, IniKey.IsMonitoringOn, 1); // after initialization monitoring should be resumed
            }

            LogInitializationStart();

            IsRtuInitialized = false;

            if (param != null)
            {
                SaveInitializationParameters(param);
                if (param.ShouldMonitoringBeStopped)
                    _rtuIni.Write(IniSection.Monitoring, IniKey.IsMonitoringOn, false);
            }

            _rtuInitializationResult = InitializeRtuManager(param);
            if (_rtuInitializationResult != ReturnCode.Ok)
            {
                callback?.Invoke();
                while (RunMainCharonRecovery() != ReturnCode.Ok){}
                return false;
            }

            if (param != null && param.Serial != _mainCharon.Serial)
            {
                _rtuIni.Write(IniSection.Monitoring, IniKey.IsMonitoringOn, false);
            }

            // permit to send heartbeats
         //   ShouldSendHeartbeat.Enqueue(new object());

            IsRtuInitialized = true;
            callback?.Invoke();

            IsMonitoringOn = _rtuIni.Read(IniSection.Monitoring, IniKey.IsMonitoringOn, 0) != 0;
            if (IsMonitoringOn || _wasMonitoringOn)
                RunMonitoringCycle();
            else
                DisconnectOtdr();

            return true;
        }

        private void LogInitializationStart()
        {
            _rtuLog.EmptyLine();

            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;
            _rtuLog.AppendLine($"RTU {_id.First6()} initialization started. Process {pid}, thread {tid}");
        }

        private void SaveInitializationParameters(InitializeRtuDto rtu)
        {
            _id = rtu.RtuId;
            _serviceIni.Write(IniSection.Server, IniKey.RtuGuid, _id.ToString());
            _rtuLog.AppendLine($@"Server sent GUID for RTU {_id.First6()}");

            _serverAddresses = (DoubleAddress)rtu.ServerAddresses.Clone();
            _serviceIni.WriteServerAddresses(_serverAddresses);
        }

        public OtauAttachedDto OtauAttachedDto { get; set; }
        public void AttachOtau(AttachOtauDto param, Action callback)
        {
            _rtuLog.EmptyLine();

            _rtuLog.AppendLine($"Check connection with OTAU {param.OtauAddresses.ToStringA()}");
            var newCharon = new Charon(param.OtauAddresses, _rtuIni, _rtuLog);
            newCharon.GetSerial();

            if (newCharon.IsLastCommandSuccessful && _mainCharon.AttachOtauToPort(param.OtauAddresses, param.OpticalPort))
            {
                _mainCharon.InitializeOtauRecursively();
                var child = _mainCharon.GetBopCharonWithLogging(param.OtauAddresses);
                OtauAttachedDto = new OtauAttachedDto()
                {
                    IsAttached = true,
                    ReturnCode = ReturnCode.OtauAttachedSuccesfully,
                    OtauId = param.OtauId,
                    RtuId = param.RtuId,
                    Serial = child.Serial,
                    PortCount = child.OwnPortCount,
                };
            }
            else
                OtauAttachedDto = new OtauAttachedDto
                {
                    IsAttached = false,
                    ReturnCode = ReturnCode.RtuAttachOtauError,
                    ErrorMessage = _mainCharon.LastErrorMessage
                };
            callback?.Invoke();
        }

        public OtauDetachedDto OtauDetachedDto { get; set; }
        public void DetachOtau(DetachOtauDto param, Action callback)
        {
            _rtuLog.EmptyLine();
            if (_mainCharon.DetachOtauFromPort(param.OpticalPort))
            {
                _mainCharon.InitializeOtauRecursively();
                OtauDetachedDto = new OtauDetachedDto()
                {
                    IsDetached = true,
                    ReturnCode = ReturnCode.OtauDetachedSuccesfully
                };
            }
            else
            {
                OtauDetachedDto = new OtauDetachedDto
                {
                    IsDetached = false,
                    ReturnCode = ReturnCode.RtuDetachOtauError,
                    ErrorMessage = _mainCharon.LastErrorMessage
                };
            }
            callback?.Invoke();
        }

        public void StartMonitoring(Action callback, bool wasMonitoringOn)
        {
            IsRtuInitialized = false;
            InitializeRtuManager(null);
            if (!wasMonitoringOn)
                _monitoringQueue.RaiseMonitoringModeChangedFlag();
            IsRtuInitialized = true;

            _rtuLog.EmptyLine();
            _rtuLog.AppendLine("RTU is turned into AUTOMATIC mode.");
            _rtuIni.Write(IniSection.Monitoring, IniKey.IsMonitoringOn, 1);
            IsMonitoringOn = true;
            callback?.Invoke();

            RunMonitoringCycle();
        }

        public void StopMonitoring(string caller)
        {
            IsMonitoringOn = false;
            _rtuLog.AppendLine($"{caller}: Interrupting current measurement...");
            _cancellationTokenSource?.Cancel();
            Thread.Sleep(TimeSpan.FromSeconds(5)); //for long measurements it could be not enough!!!
        }

        public void ChangeSettings(ApplyMonitoringSettingsDto settings, Action callback)
        {
            var wasMonitoringOn = IsMonitoringOn;
            if (IsMonitoringOn)
                StopMonitoring("Apply monitoring settings");

            ApplyChangeSettings(settings, callback, wasMonitoringOn);

            if (!settings.IsMonitoringOn)
                callback?.Invoke();
        }

        private void ApplyChangeSettings(ApplyMonitoringSettingsDto dto, Action callback, bool wasMonitoringOn)
        {
            _rtuLog.EmptyLine();
            _rtuLog.AppendLine("Start ApplyChangeSettings");

            ApplyNewFrequenciesToIni(dto.Timespans);
            _monitoringQueue.ComposeNewQueue(dto.Ports);
            _rtuLog.AppendLine($"Queue merged. {_monitoringQueue.Count()} port(s) in queue");
            _monitoringQueue.Save();

            if (dto.IsMonitoringOn)
                StartMonitoring(callback, wasMonitoringOn);
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


        private bool ToggleToPort(OtauPortDto port)
        {
//            if (port.OtauTcpPort == 23)
//                port.OtauIp = _rtuIni.Read(IniSection.RtuManager, IniKey.OtdrIp, "192.168.88.101");
//            var toggleResult = _mainCharon.SetExtendedActivePort(new NetAddress(port.OtauIp, port.OtauTcpPort), port.OpticalPort);
            var toggleResult = _mainCharon.SetExtendedActivePort(port.Serial, port.OpticalPort);

            return toggleResult == CharonOperationResult.Ok;
        }
    }
}
