using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
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
        // public ConcurrentQueue<object> WcfCommandsQueue = new ConcurrentQueue<object>();
        public readonly ConcurrentQueue<object> ShouldSendHeartbeat = new ConcurrentQueue<object>();

        public void Initialize(InitializeRtuDto param, Action callback)
        {
            if (IsMonitoringOn || _wasMonitoringOn)
            {
                _wasMonitoringOn = IsMonitoringOn;
                StopMonitoring("Initialize");
            }

            if (param != null)
            {
                IsMonitoringOn = false;
                _wasMonitoringOn = false;
                _rtuIni.Write(IniSection.Monitoring, IniKey.IsMonitoringOn, false);
                _rtuLog.AppendLine("Initialization by the USER puts RTU into MANUAL mode.");
            }
            IsAutoBaseMeasurementInProgress = false;
            _rtuIni.Write(IniSection.Monitoring, IniKey.IsAutoBaseMeasurementInProgress, false);


            LogInitializationStart();

            IsRtuInitialized = false;

            if (param != null)
            {
                SaveInitializationParameters(param);
            }

            _rtuInitializationResult = InitializeRtuManager(param);
            if (_rtuInitializationResult != ReturnCode.Ok && param == null)
            {
                while (RunMainCharonRecovery() != ReturnCode.Ok) { }
            }

            _treeOfAcceptableMeasParams = _otdrManager.InterOpWrapper.GetTreeOfAcceptableMeasParams();

            IsRtuInitialized = true;
            callback?.Invoke();

            IsMonitoringOn = _rtuIni.Read(IniSection.Monitoring, IniKey.IsMonitoringOn, false);
            if (IsMonitoringOn || _wasMonitoringOn)
                RunMonitoringCycle();
            else
                DisconnectOtdr();
        }

        private void LogInitializationStart()
        {
            _rtuLog.EmptyLine();

            var pid = Process.GetCurrentProcess().Id;
            var tid = Thread.CurrentThread.ManagedThreadId;
            _rtuLog.AppendLine($"RTU {_id.First6()} initialization started");
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

            var newCharon = _mainCharon.AttachOtauToPort(param.NetAddress, param.OpticalPort);
            if (newCharon != null)
            {
                _rtuLog.AppendLine($"Otau {param.NetAddress.ToStringA()} attached to port {param.OpticalPort} and has {newCharon.OwnPortCount} ports");
                OtauAttachedDto = new OtauAttachedDto()
                {
                    IsAttached = true,
                    ReturnCode = ReturnCode.OtauAttachedSuccessfully,
                    OtauId = param.OtauId,
                    RtuId = param.RtuId,
                    Serial = newCharon.Serial,
                    PortCount = newCharon.OwnPortCount,
                };
            }
            else
                OtauAttachedDto = new OtauAttachedDto
                {
                    IsAttached = false,
                    ReturnCode = ReturnCode.RtuAttachOtauError,
                    ErrorMessage = _mainCharon.LastErrorMessage
                };

            _rtuLog.AppendLine($"Now RTU has {_mainCharon.OwnPortCount}/{_mainCharon.FullPortCount} ports");
            callback?.Invoke();
        }

        public OtauDetachedDto OtauDetachedDto { get; set; }
        public void DetachOtau(DetachOtauDto param, Action callback)
        {
            _rtuLog.EmptyLine();
            if (_mainCharon.DetachOtauFromPort(param.OpticalPort))
            {
                // _mainCharon.InitializeOtauRecursively();
                _rtuLog.AppendLine($"Otau {param.NetAddress.ToStringA()} detached from port {param.OpticalPort}");
                _rtuLog.AppendLine($"Now RTU has {_mainCharon.OwnPortCount}/{_mainCharon.FullPortCount} ports");

                OtauDetachedDto = new OtauDetachedDto()
                {
                    IsDetached = true,
                    ReturnCode = ReturnCode.OtauDetachedSuccessfully
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

        private void StartMonitoring(Action callback, bool wasMonitoringOn)
        {
            IsRtuInitialized = false;
            _rtuInitializationResult = InitializeRtuManager(null);
            bool isCallbackReturned = false;
            if (_rtuInitializationResult != ReturnCode.Ok)
            {
                while (RunMainCharonRecovery() != ReturnCode.Ok) { }
            }
            else
            {
                callback?.Invoke();
                isCallbackReturned = true;
            }

            if (!wasMonitoringOn)
                _monitoringQueue.RaiseMonitoringModeChangedFlag();
            IsRtuInitialized = true;

            _rtuLog.EmptyLine();
            _rtuLog.AppendLine("RTU is turned into AUTOMATIC mode.");
            _rtuIni.Write(IniSection.Monitoring, IniKey.LastMeasurementTimestamp, DateTime.Now.ToString(CultureInfo.CurrentCulture));
            _rtuIni.Write(IniSection.Monitoring, IniKey.IsMonitoringOn, true);
            IsMonitoringOn = true;

            if (!isCallbackReturned)
                callback?.Invoke();

            RunMonitoringCycle();
        }

        public void StopMonitoringRequest(string caller)
        {
            StopMonitoring(caller);
            _rtuIni.Write(IniSection.Monitoring, IniKey.IsMonitoringOn, false);
        }

        private void StopMonitoring(string caller)
        {
            IsMonitoringOn = false;
            _rtuLog.AppendLine($"{caller}: Interrupting current measurement...");
            _cancellationTokenSource?.Cancel();

            // if Lmax = 240km and Time = 10min one step lasts 5-6 sec
            Thread.Sleep(TimeSpan.FromSeconds(6));
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
            _monitoringQueue.SaveBackup();

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


        // private bool ToggleToPort(OtauPortDto port)
        // {
        //     var toggleResult = _mainCharon.SetExtendedActivePort(port.Serial, port.OpticalPort);
        //     _rtuLog.AppendLine(toggleResult == CharonOperationResult.Ok ? "Toggled Ok." : "Failed to toggle to port");
        //     return toggleResult == CharonOperationResult.Ok;
        // }

        private CharonOperationResult ToggleToPort2(OtauPortDto port)
        {
            var toggleResult = _mainCharon.SetExtendedActivePort(port.Serial, port.OpticalPort);
            _rtuLog.AppendLine(toggleResult == CharonOperationResult.Ok
                ? "Toggled Ok."
                : toggleResult == CharonOperationResult.MainOtauError
                    ? "Failed to toggle to main otau port" : "Failed to toggle to additional otau port");

            if (toggleResult == CharonOperationResult.AdditionalOtauError)
            {
                var connectionTimeout = _rtuIni.Read(IniSection.Charon, IniKey.ConnectionTimeout, 30);
                var mikrotik = new MikrotikInBop(_rtuLog, port.NetAddress.Ip4Address, connectionTimeout);
                try
                {
                    if (mikrotik.Connect())
                        mikrotik.Reboot();
                }
                catch (Exception e)
                {
                    _rtuLog.AppendLine($"Cannot connect Mikrotik {port.NetAddress.Ip4Address}" + e.Message);
                }
            }

            return toggleResult;
        }
    }
}
