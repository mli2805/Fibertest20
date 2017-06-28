using System;
using Iit.Fibertest.Utils35;

namespace RtuManagement
{
    public partial class RtuManager
    {
        private TimeSpan _mikrotikRebootTimeout;

        private void ClearArp()
        {
            var logLevel = _serviceIni.Read(IniSection.General, IniKey.LogLevel, 1);
            var res = Arp.GetTable();
            if (logLevel == 3)
                _serviceLog.AppendLine(res);
            res = Arp.ClearCache();
            _rtuLog.AppendLine($"Clear ARP table - {res}");
            _serviceLog.AppendLine($"Clear ARP table - {res}");
            res = Arp.GetTable();
            if (logLevel == 3)
                _serviceLog.AppendLine(res);
        }

        private void RunMainCharonRecovery()
        {
            var step = (RecoveryStep)_rtuIni.Read(IniSection.Recovering, IniKey.RecoveryStep, (int)RecoveryStep.Ok);

            switch (step)
            {
                case RecoveryStep.Ok:
                    _rtuIni.Write(IniSection.Recovering, IniKey.RecoveryStep, (int)RecoveryStep.ClearArp);
                    ClearArp();
                    return;
                case RecoveryStep.ClearArp:
                    _rtuIni.Write(IniSection.Recovering, IniKey.RecoveryStep, (int)RecoveryStep.RestartService);
                    _rtuLog.AppendLine("Exit rtu service");
                    _serviceLog.AppendLine("Exit rtu service");
                    Environment.Exit(1);
                    return;
                case RecoveryStep.RestartService:
                    var enabled = _rtuIni.Read(IniSection.Recovering, IniKey.RebootSystemEnabled, false);
                    if (enabled)
                    {
                        _rtuIni.Write(IniSection.Recovering, IniKey.RecoveryStep, (int)RecoveryStep.RebootPc);
                        var delay = _rtuIni.Read(IniSection.Recovering, IniKey.RebootSystemDelay, 30);
                        _serviceLog.AppendLine("Reboot system");
                        RestoreFunctions.RebootSystem(_rtuLog, delay);
                        Environment.Exit(0);
                    }
                    else
                    {
                        _rtuIni.Write(IniSection.Recovering, IniKey.RecoveryStep, (int)RecoveryStep.ClearArp);
                        ClearArp();
                    }
                    return;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="damagedOtau">check in damaged otau list, while initialization is surely is null</param>
        /// <param name="damagedOtauIp"></param>
        private void RunAdditionalOtauRecovery(DamagedOtau damagedOtau, string damagedOtauIp)
        {
            if (damagedOtau != null)
            {
                damagedOtau.RebootStarted = DateTime.Now;
                damagedOtau.RebootAttempts++;
            }
            else
            {
                damagedOtau = new DamagedOtau(damagedOtauIp);
                _damagedOtaus.Add(damagedOtau);
            }

            _rtuLog.AppendLine($"Reboot attempt N{damagedOtau.RebootAttempts}");
            var mikrotik = new MikrotikInBop(_rtuLog, damagedOtauIp);
            if (mikrotik.Connect())
                mikrotik.Reboot();
        }
    }
}
