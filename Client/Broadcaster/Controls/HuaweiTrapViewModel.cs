using System;
using System.Threading.Tasks;
using Caliburn.Micro;
using Iit.Fibertest.UtilsLib;

namespace Broadcaster
{
    public class HuaweiTrapViewModel : PropertyChangedBase
    {
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;

        private DateTime _startTime;

        public int TrapCount { get; set; } = 10;
        public int PauseMs { get; set; } = 2000;

        public HuaweiTrapViewModel(IniFile iniFile, IMyLog logFile)
        {
            _iniFile = iniFile;
            _logFile = logFile;
            _startTime = DateTime.Now;
        }

        public async void Send()
        {
            var snmpAgent = new SnmpAgent(_iniFile, _logFile);
            var sh = new SnmpHuaweiAgent(snmpAgent);
            for (int i = 0; i < TrapCount; i++)
            {
                var unused = sh.SendV2CPonTestTrap(_startTime, i+1);
                await Task.Delay(PauseMs);
            }
        }

    }
}
