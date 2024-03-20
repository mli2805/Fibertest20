using System;

namespace Iit.Fibertest.Dto
{
    public class RtuGeneralConfig
    {
        public string LogLevelMinimum { get; set; } = "Information";
        public string LogRollingInterval { get; set; } = "Month";

        public DoubleAddress ServerAddress { get; set; } = new DoubleAddress();
        public Guid RtuId { get; set; }
        public int PreviousOwnPortCount { get; set; } = -1;
        public string OtdrIp { get; set; } = "192.168.88.101";
        public string OtauIp { get; set; } = "192.168.88.101";
        public int OtdrTcpPort { get; set; } = 1500;
        public int OtauTcpPort { get; set; } = 23;

        // public int RtuPauseAfterReboot { get; set; } = 20;
        // public int RtuUpTimeForAdditionalPause { get; set; } = 100;

    }
}
