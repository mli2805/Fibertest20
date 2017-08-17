using System;
using System.Threading;
using Iit.Fibertest.UtilsLib;

namespace RtuManagement
{
    public partial class RtuManager
    {
        private readonly TimeSpan _mikrotikRebootTimeout;

        private void ClearArp()
        {
            var logLevel = _serviceIni.Read(IniSection.General, IniKey.LogLevel, 1);
            var res = Arp.GetTable();
            if (logLevel == 3)
                _serviceLog.AppendLine(res);
            Arp.ClearCache();
            _rtuLog.AppendLine("Recovery procedure: Clear ARP table.");
            _serviceLog.AppendLine("Recovery procedure: Clear ARP table.");
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
                    _rtuLog.AppendLine("Recovery procedure: Exit rtu service.");
                    _serviceLog.AppendLine("Recovery procedure: Exit rtu service.");
                    Environment.FailFast("Recovery procedure: Exit rtu service.");
                    return;
                case RecoveryStep.RestartService:
                    var enabled = _rtuIni.Read(IniSection.Recovering, IniKey.RebootSystemEnabled, false);
                    if (enabled)
                    {
                        _rtuIni.Write(IniSection.Recovering, IniKey.RecoveryStep, (int)RecoveryStep.ClearArp);
                        var delay = _rtuIni.Read(IniSection.Recovering, IniKey.RebootSystemDelay, 60);
                        _serviceLog.AppendLine("Recovery procedure: Reboot system.");
                        RestoreFunctions.RebootSystem(_rtuLog, delay);
                        Thread.Sleep(TimeSpan.FromSeconds(delay+5));
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

            _serviceLog.AppendLine($"Mikrotik {damagedOtauIp} reboot N{damagedOtau.RebootAttempts}");
            _rtuLog.AppendLine($"Reboot attempt N{damagedOtau.RebootAttempts}");
            var mikrotik = new MikrotikInBop(_rtuLog, damagedOtauIp);
            if (mikrotik.Connect())
                mikrotik.Reboot();
        }
    }
}
