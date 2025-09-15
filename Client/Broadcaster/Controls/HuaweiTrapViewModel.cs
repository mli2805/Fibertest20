using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
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
        private DateTime _startTime = DateTime.Now;

        public int TrapCount { get; set; } = 3;
        public int PauseMs { get; set; } = 10;

        public async void Send()
        {
            var json = File.ReadAllText(@"../ini/snmp-settings.json");
            var snmp = JsonSerializer.Deserialize<SnmpNewSettings>(json);
            var huaweiSnmpService = new HuaweiSnmpService();

            for (int i = 0; i < TrapCount; i++)
            {
                huaweiSnmpService.SendV2CPonTestTrap(snmp, _startTime, 1, 2, i + 1);
                await Task.Delay(PauseMs);
                // var unused1 = huaweiSnmpService.SendV2CPonTestTrap(_startTime, 128, 2, i+1);
                // await Task.Delay(PauseMs);
                // var unused2 = huaweiSnmpService.SendV2CPonTestTrap(_startTime, 0, 5, i+1);
                // await Task.Delay(PauseMs);
            }
        }
    }
}
