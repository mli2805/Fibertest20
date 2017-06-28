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

    }
}
