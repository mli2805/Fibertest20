using System;

namespace Iit.Fibertest.Dto
{
    public class RtuGeneralConfig
    {
        public string Version { get; set; }
        public Guid RtuId { get; set; }
        public int PreviousOwnPortCount { get; set; } = -1;
        public string OtdrIp { get; set; } = "192.168.88.101";
        public string OtauIp { get; set; } = "192.168.88.101";
        public int OtdrTcpPort { get; set; } = 1500;
        public int OtauTcpPort { get; set; } = 23;
        public DoubleAddress ServerAddress { get; set; } = new DoubleAddress();

        // public int RtuPauseAfterReboot { get; set; } = 20;
        // public int RtuUpTimeForAdditionalPause { get; set; } = 100;

    }
}
