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
        private ReturnCode InitializeRtuManager()
        {
            LedDisplay.Show(_rtuIni, _rtuLog, LedDisplayCode.Connecting);

            var otdrInitializationResult = InitializeOtdr();
            if (otdrInitializationResult != ReturnCode.Ok)
            {
                // We can't be here because InitializeOtdr should endlessly continue
                // until Otdr is initialized Ok.
                // (It's senseless to work without OTDR.)
                _rtuLog.AppendLine("Otdr initialization failed.");
                return otdrInitializationResult;
            }

            var otauInitializationResult = InitializeOtau();

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

            var otdrAddress = _rtuIni.Read(IniSection.General, IniKey.OtdrIp, DefaultIp);
            Thread.Sleep(3000);
            var res = _otdrManager.ConnectOtdr(otdrAddress);
            if (!res)
            {
                RunMainCharonRecovery(); // one of recovery steps inevitably exits process
                res = _otdrManager.ConnectOtdr(otdrAddress);
                if (!res)
                    RunMainCharonRecovery(); // one of recovery steps inevitably exits process
            }
            _rtuIni.Write(IniSection.Recovering, IniKey.RecoveryStep, 0);
            return ReturnCode.Ok;
        }

        private ReturnCode InitializeOtau()
        {
            var otauIpAddress = _rtuIni.Read(IniSection.General, IniKey.OtauIp, DefaultIp);
            _mainCharon = new Charon(new NetAddress(otauIpAddress, 23), _rtuIni, _rtuLog);
            var res = _mainCharon.InitializeOtau();

            if (res == null)
                return ReturnCode.Ok;

            if (!res.Equals(_mainCharon.NetAddress))
            {
                RunAdditionalOtauRecovery(null, res.Ip4Address);
                return ReturnCode.Ok;
            }
            else // main charon
            {
                RunMainCharonRecovery(); // one of recovery steps inevitably exits process
                res = _mainCharon.InitializeOtau();
                if (res != null)
                    RunMainCharonRecovery(); // one of recovery steps inevitably exits process
            }
            return ReturnCode.Ok;
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
            var otdrAddress = _rtuIni.Read(IniSection.General, IniKey.OtdrIp, DefaultIp);
            _otdrManager.DisconnectOtdr(otdrAddress);
            _rtuLog.AppendLine("Rtu is in MANUAL mode.");
        }

    }
}
