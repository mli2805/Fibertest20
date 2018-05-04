using System;
using System.Threading;
using Iit.Fibertest.DirectCharonLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.IitOtdrLibrary;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.RtuManagement
{
    public partial class RtuManager
    {
        private ReturnCode InitializeRtuManager(InitializeRtuDto dto)
        {
            RestoreFunctions.ResetCharonThroughComPort(_rtuIni, _rtuLog);
            LedDisplay.Show(_rtuIni, _rtuLog, LedDisplayCode.Connecting);

            var otdrInitializationResult = InitializeOtdr();
            if (otdrInitializationResult != ReturnCode.Ok)
            {
                _rtuLog.AppendLine("OTDR initialization failed.");
                LedDisplay.Show(_rtuIni, _rtuLog, LedDisplayCode.ErrorConnectOtdr);
                return otdrInitializationResult;
            }

            var otauInitializationResult = dto != null
                ? ReInitializeOtauOnUsersRequest(dto)
                : InitializeOtau();
            if (otauInitializationResult != ReturnCode.Ok)
            {
                _rtuLog.AppendLine("OTAU initialization failed.");
                LedDisplay.Show(_rtuIni, _rtuLog, LedDisplayCode.ErrorConnectOtau);
                return otauInitializationResult;
            }

            _mainCharon.ShowOnDisplayMessageReady();

            _monitoringQueue = new MonitoringQueue(_rtuLog);
            _monitoringQueue.Load();
            GetMonitoringParams();

            _rtuLog.AppendLine("Rtu Manager initialized successfully.");
            return otauInitializationResult;
        }

        private ReturnCode InitializeOtdr()
        {
            _otdrManager = new OtdrManager(@"OtdrMeasEngine\", _rtuIni, _rtuLog);
            if (_otdrManager.LoadDll() != "")
                return ReturnCode.OtdrInitializationCannotLoadDll;

            if (!_otdrManager.InitializeLibrary())
                return ReturnCode.OtdrInitializationCannotInitializeDll;

            var otdrAddress = _rtuIni.Read(IniSection.RtuManager, IniKey.OtdrIp, DefaultIp);
            Thread.Sleep(300);
            return _otdrManager.ConnectOtdr(otdrAddress) ? ReturnCode.Ok : ReturnCode.OtdrCannontConnect;
        }

        private ReturnCode ReInitializeOtauOnUsersRequest(InitializeRtuDto dto)
        {
            if (!_mainCharon.IsBopSupported)
                return dto.Children.Count > 0 ? ReturnCode.RtuDoesntSupportBop : ReturnCode.Ok;

            // detach bops if they are not attached in client
            foreach (var pair in _mainCharon.Children)
            {
                if (!dto.Children.ContainsKey(pair.Key) ||
                    pair.Value.Serial != dto.Children[pair.Key].Serial)
                {
                    _mainCharon.DetachOtauFromPort(pair.Key);
                }
            }

            // attach bops if they are attached in client (even if there are no such bop in reality)
            // initialize all child bops to get their states
            foreach (var pair in dto.Children)
            {
                if (!_mainCharon.Children.ContainsKey(pair.Key))
                    _mainCharon.AttachOtauToPort(pair.Value.NetAddress, pair.Key);
            }

            return InitializeOtau();
        }
        private ReturnCode InitializeOtau()
        {
            var otauIpAddress = _rtuIni.Read(IniSection.RtuManager, IniKey.OtauIp, DefaultIp);
            _mainCharon = new Charon(new NetAddress(otauIpAddress, 23), _rtuIni, _rtuLog);
            var res = _mainCharon.InitializeOtauRecursively();

            var previousOwnPortCount = _rtuIni.Read(IniSection.RtuManager, IniKey.PreviousOwnPortCount, -1);
            if (previousOwnPortCount == -1)
                _rtuIni.Write(IniSection.RtuManager, IniKey.PreviousOwnPortCount, _mainCharon.OwnPortCount);
            if (previousOwnPortCount != _mainCharon.OwnPortCount)
            {
                if (_mainCharon.OwnPortCount != 1) // OTAU is changed - not broken
                {
                    _rtuIni.Write(IniSection.RtuManager, IniKey.PreviousOwnPortCount, _mainCharon.OwnPortCount);
                    _mainCharon.IsOk = true;
                }
                else
                {
                    _mainCharon.IsOk = false;
                }
            }

            return res == null ? ReturnCode.Ok : ReturnCode.OtauInitializationError;
        }

        private void GetMonitoringParams()
        {
            _preciseMakeTimespan =
                TimeSpan.FromSeconds(_rtuIni.Read(IniSection.Monitoring, IniKey.PreciseMakeTimespan, 3600));
            _preciseSaveTimespan =
                TimeSpan.FromSeconds(_rtuIni.Read(IniSection.Monitoring, IniKey.PreciseSaveTimespan, 3600));
            _fastSaveTimespan =
                TimeSpan.FromSeconds(_rtuIni.Read(IniSection.Monitoring, IniKey.FastSaveTimespan, 3600));
        }

        private void DisconnectOtdr()
        {
            var otdrAddress = _rtuIni.Read(IniSection.RtuManager, IniKey.OtdrIp, DefaultIp);
            _otdrManager.DisconnectOtdr(otdrAddress);
            _rtuLog.AppendLine("Rtu is in MANUAL mode.");
        }

    }
}
