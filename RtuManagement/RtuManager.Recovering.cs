using System;
using Iit.Fibertest.Utils35;

namespace RtuManagement
{
    public partial class RtuManager
    {
        private TimeSpan _mikrotikRebootTimeout;

        private void RestoreOtdrConnection()
        {
            var arp = _rtuIni.Read(IniSection.Restore, IniKey.ClearArp, 0);
            if (arp != 0)
            {
                _rtuIni.Write(IniSection.Restore, IniKey.ClearArp, 0);
                ClearArp();
            }

            var reboot = _rtuIni.Read(IniSection.Restore, IniKey.RebootSystem, 0);
            if (reboot != 0)
            {
                _rtuIni.Write(IniSection.Restore, IniKey.RebootSystem, 0);
                RestoreFunctions.RebootSystem(_rtuLog);
            }
        }

        private void ClearArp()
        {
            var res = Arp.GetTable();
            _rtuLog.AppendLine(res);
            res = Arp.ClearCache();
            _rtuLog.AppendLine($"Clear ARP table - {res}");
            res = Arp.GetTable();
            _rtuLog.AppendLine(res);
        }

    }
}
