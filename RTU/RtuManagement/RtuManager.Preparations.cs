using System;
using System.Collections.Generic;
using System.IO;
using Iit.Fibertest.DirectCharonLibrary;
using Iit.Fibertest.IitOtdrLibrary;
using Iit.Fibertest.Utils35;

namespace RtuManagement
{
    public partial class RtuManager
    {
        private CharonOperationResult InitializeRtuManager()
        {
            LedDisplay.Show(_rtuIni, _rtuLog, LedDisplayCode.Connecting);

            if (!InitializeOtdr())
            {
                // We can't be here because InitializeOtdr should endlessly continue
                // until Otdr is initialized Ok.
                // (It's senseless to work without OTDR.)
                _rtuLog.AppendLine("Otdr initialization failed.");
                return CharonOperationResult.OtdrError;
            }

            var result = InitializeOtau();

            _mainCharon.ShowOnDisplayMessageReady();

            GetMonitoringQueue();
            GetMonitoringParams();

            _rtuLog.AppendLine("Rtu Manager initialization failed.");
            return result;
        }

        private bool InitializeOtdr()
        {
            _otdrManager = new OtdrManager(@"OtdrMeasEngine\", _rtuIni, _rtuLog);
            if (_otdrManager.LoadDll() != "")
                return false;

            if (!_otdrManager.InitializeLibrary())
                return false;

            var otdrAddress = _rtuIni.Read(IniSection.General, IniKey.OtdrIp, DefaultIp);
            var res = _otdrManager.ConnectOtdr(otdrAddress);
            if (!res)
            {
                RunMainCharonRecovery(); // one of recovery steps inevitably exits process
                res = _otdrManager.ConnectOtdr(otdrAddress);
                if (!res)
                    RunMainCharonRecovery(); // one of recovery steps inevitably exits process
            }
            _rtuIni.Write(IniSection.Recovering, IniKey.RecoveryStep, 0);
            return true;
        }

        private CharonOperationResult InitializeOtau()
        {
            var otauIpAddress = _rtuIni.Read(IniSection.General, IniKey.OtauIp, DefaultIp);
            _mainCharon = new Charon(new NetAddress(otauIpAddress, 23), _rtuIni, _rtuLog);
            var res = _mainCharon.InitializeOtau();

            if (res == null)
                return CharonOperationResult.Ok;

            if (!res.Equals(_mainCharon.NetAddress))
            {
                RunAdditionalOtauRecovery(null, res.Ip4Address);
                return CharonOperationResult.Ok;
            }
            else // main charon
            {
                RunMainCharonRecovery(); // one of recovery steps inevitably exits process
                res = _mainCharon.InitializeOtau();
                if (res != null)
                    RunMainCharonRecovery(); // one of recovery steps inevitably exits process
            }
            return CharonOperationResult.Ok;
        }

        private void GetMonitoringQueue()
        {
            _rtuLog.EmptyLine();
            _rtuLog.AppendLine("Monitoring queue assembling...");
            _monitoringQueue = new Queue<ExtendedPort>();
            var monitoringSettingsFile = Utils.FileNameForSure(@"..\ini\", @"monitoring.que", false);
            var content = File.ReadAllLines(monitoringSettingsFile);
            foreach (var line in content)
            {
                var extendedPort = ExtendedPort.Create(line, _rtuLog);
                if (extendedPort != null && _mainCharon.IsExtendedPortValidForMonitoring(extendedPort))
                    _monitoringQueue.Enqueue(extendedPort);
            }
            _rtuLog.AppendLine($"{_monitoringQueue.Count} port(s) in queue.");
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
