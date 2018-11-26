using System;

namespace Iit.Fibertest.RtuManagement
{
    public class DamagedOtau
    {
        public string Ip { get; set; }
        public int TcpPort { get; set; }
        public string Serial { get; set; }
        public DateTime RebootStarted { get; set; }
        public int RebootAttempts { get; set; }

        public DamagedOtau(string ip, int tcpPort, string serial)
        {
            Ip = ip;
            TcpPort = tcpPort;
            Serial = serial;
            RebootStarted = DateTime.Now;
            RebootAttempts = 0;
        }
    }
}
