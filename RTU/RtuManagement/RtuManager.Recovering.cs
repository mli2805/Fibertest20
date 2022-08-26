using System;
using System.Threading;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.RtuManagement
{
    public partial class RtuManager
    {
        private readonly TimeSpan _mikrotikRebootTimeout;

        private ReturnCode RunMainCharonRecovery()
        {
            // see issue 713
            //if (!IsMonitoringOn && !_wasMonitoringOn) // see issue 680
            //{
            //    return InitializeRtuManager(null); // Reset Charon
            //}

            var previousStep = (RecoveryStep)_serviceIni.Read(IniSection.Recovering, IniKey.RecoveryStep, (int)RecoveryStep.Ok);

            switch (previousStep)
            {
                case RecoveryStep.Ok:
                    _serviceIni.Write(IniSection.Recovering, IniKey.RecoveryStep, (int)RecoveryStep.ResetArpAndCharon);
                    RestoreFunctions.ClearArp(_serviceLog, _rtuLog);
                    var recoveryResult = InitializeRtuManager(null);
                    if (recoveryResult == ReturnCode.Ok)
                        _serviceIni.Write(IniSection.Recovering, IniKey.RecoveryStep, (int)RecoveryStep.Ok);
                    return recoveryResult; // Reset Charon
                case RecoveryStep.ResetArpAndCharon:
                    _serviceIni.Write(IniSection.Recovering, IniKey.RecoveryStep, (int)RecoveryStep.RestartService);
                    _rtuLog.AppendLine("Recovery procedure: Exit rtu service.");
                    _serviceLog.AppendLine("Recovery procedure: Exit rtu service.");
                    Environment.FailFast("Recovery procedure: Exit rtu service.");
                    //                    Environment.Exit(1); // ваще не выходит
                    //  new ServiceController("FibertestRtuService").Stop(); // медленно выходит, успевает выполнить еще несколько операторов
                    // ReSharper disable once HeuristicUnreachableCode
                    return ReturnCode.Ok;
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
                        return ReturnCode.Ok;
                    }
                    else
                    {
                        _serviceIni.Write(IniSection.Recovering, IniKey.RecoveryStep, (int)RecoveryStep.ResetArpAndCharon);
                        RestoreFunctions.ClearArp(_serviceLog, _rtuLog);
                        var recoveryResult1 = InitializeRtuManager(null);
                        if (recoveryResult1 == ReturnCode.Ok)
                            _serviceIni.Write(IniSection.Recovering, IniKey.RecoveryStep, (int)RecoveryStep.Ok);
                        return recoveryResult1;
                    }
                case RecoveryStep.RebootPc:
                    _serviceIni.Write(IniSection.Recovering, IniKey.RecoveryStep, (int)RecoveryStep.ResetArpAndCharon);
                    RestoreFunctions.ClearArp(_serviceLog, _rtuLog);
                    var recoveryResult2 = InitializeRtuManager(null);
                    if (recoveryResult2 == ReturnCode.Ok)
                        _serviceIni.Write(IniSection.Recovering, IniKey.RecoveryStep, (int)RecoveryStep.Ok);
                    return recoveryResult2;
            }

            return ReturnCode.Ok;
        }


        private void RunAdditionalOtauRecovery(DamagedOtau damagedOtau)
        {
            damagedOtau.RebootStarted = DateTime.Now;
            damagedOtau.RebootAttempts++;

            var mikrotikRebootAttemptsBeforeNotification = _rtuIni.Read(IniSection.Recovering, IniKey.MikrotikRebootAttemptsBeforeNotification, 3);
            if (damagedOtau.RebootAttempts == mikrotikRebootAttemptsBeforeNotification)
                SendByMsmq(new BopStateChangedDto()
                {
                    RtuId = _id,
                    OtauIp = damagedOtau.Ip,
                    TcpPort = damagedOtau.TcpPort,
                    Serial = damagedOtau.Serial,
                    IsOk = false
                });
            // SendByMsmq(new BopStateChangedDto() { RtuId = _id, Serial = damagedOtau.Serial, IsOk = false });

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
