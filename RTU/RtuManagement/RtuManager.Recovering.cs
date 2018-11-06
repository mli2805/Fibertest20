using System;
using System.Threading;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using System.ServiceProcess;

namespace Iit.Fibertest.RtuManagement
{
    public partial class RtuManager
    {
        private readonly TimeSpan _mikrotikRebootTimeout;

        private void RunMainCharonRecovery()
        {
            var previousStep = (RecoveryStep)_serviceIni.Read(IniSection.Recovering, IniKey.RecoveryStep, (int)RecoveryStep.Ok);

            switch (previousStep)
            {
                case RecoveryStep.Ok:
                    _serviceIni.Write(IniSection.Recovering, IniKey.RecoveryStep, (int)RecoveryStep.ResetArpAndCharon);
                    RestoreFunctions.ClearArp(_serviceLog, _rtuLog);
                    InitializeRtuManager(null); // Reset Charon
                    return;
                case RecoveryStep.ResetArpAndCharon:
                    _serviceIni.Write(IniSection.Recovering, IniKey.RecoveryStep, (int)RecoveryStep.RestartService);
                    _rtuLog.AppendLine("Recovery procedure: Exit rtu service.");
                    _serviceLog.AppendLine("Recovery procedure: Exit rtu service.");
                   // Environment.FailFast("Recovery procedure: Exit rtu service.");
//                    Environment.Exit(1); // ваще не выходит
                    var svc = new ServiceController("FibertestRtuService");
                    svc.Stop();
                    return;
                case RecoveryStep.RestartService:
                    var enabled = _serviceIni.Read(IniSection.Recovering, IniKey.RebootSystemEnabled, false);
                    if (enabled)
                    {
                        _serviceIni.Write(IniSection.Recovering, IniKey.RecoveryStep, (int)RecoveryStep.RebootPc);
                        var delay = _serviceIni.Read(IniSection.Recovering, IniKey.RebootSystemDelay, 60);
                        _rtuLog.AppendLine("Recovery procedure: Reboot system.");
                        _serviceLog.AppendLine("Recovery procedure: Reboot system.");
                        RestoreFunctions.RebootSystem(_serviceLog, delay);
                        Thread.Sleep(TimeSpan.FromSeconds(delay + 5));
                    }
                    else
                    {
                        _serviceIni.Write(IniSection.Recovering, IniKey.RecoveryStep, (int)RecoveryStep.ResetArpAndCharon);
                        RestoreFunctions.ClearArp(_serviceLog, _rtuLog);
                        InitializeRtuManager(null);
                    }
                    return;
                case RecoveryStep.RebootPc:
                    _serviceIni.Write(IniSection.Recovering, IniKey.RecoveryStep, (int)RecoveryStep.ResetArpAndCharon);
                    RestoreFunctions.ClearArp(_serviceLog, _rtuLog);
                    InitializeRtuManager(null);
                    return;
            }
        }


        private void RunAdditionalOtauRecovery(DamagedOtau damagedOtau)
        {
            damagedOtau.RebootStarted = DateTime.Now;
            damagedOtau.RebootAttempts++;

            var mikrotikRebootAttemptsBeforeNotification = _rtuIni.Read(IniSection.Recovering, IniKey.MikrotikRebootAttemptsBeforeNotification, 1);
            if (damagedOtau.RebootAttempts == mikrotikRebootAttemptsBeforeNotification)
                SendByMsmq(new BopStateChangedDto() { RtuId = _id, OtauIp = damagedOtau.Ip, TcpPort = damagedOtau.TcpPort, IsOk = false });

            _serviceLog.AppendLine($"Mikrotik {damagedOtau.Ip} reboot N{damagedOtau.RebootAttempts}");
            _rtuLog.AppendLine($"Reboot attempt N{damagedOtau.RebootAttempts}");
            var connectionTimeout = _rtuIni.Read(IniSection.Charon, IniKey.ConnectionTimeout, 30);
            var mikrotik = new MikrotikInBop(_rtuLog, damagedOtau.Ip, connectionTimeout);
            try
            {
                if (mikrotik.Connect())
                    mikrotik.Reboot();
            }
            catch (Exception e)
            {
                _rtuLog.AppendLine($"Cannot connect Mikrotik {damagedOtau.Ip}" + e.Message);
            }

        }
    }
}
