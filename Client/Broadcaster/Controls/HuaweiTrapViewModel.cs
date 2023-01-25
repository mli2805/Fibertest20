using System;
using System.Threading.Tasks;
using Caliburn.Micro;
using Iit.Fibertest.UtilsLib;
using Utils471;

namespace Broadcaster
{
    public class PayLoad
    {
        public int Id;
        public int Square;
    }
    public class HuaweiTrapViewModel : PropertyChangedBase
    {
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;

        private DateTime _startTime;

        public int TrapCount { get; set; } = 3;
        public int PauseMs { get; set; } = 10;

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
                var unused = sh.SendV2CPonTestTrap(_startTime, 1, 2, i + 1);
                await Task.Delay(PauseMs);
                // var unused1 = sh.SendV2CPonTestTrap(_startTime, 128, 2, i+1);
                // await Task.Delay(PauseMs);
                // var unused2 = sh.SendV2CPonTestTrap(_startTime, 0, 5, i+1);
                // await Task.Delay(PauseMs);
            }
        }

    }
}
