using System;

namespace Iit.Fibertest.RtuManagement
{
    public class DamagedOtau
    {
        public string Ip { get; set; }
        public int TcpPort { get; set; }
        public DateTime RebootStarted { get; set; }
        public int RebootAttempts { get; set; }

        public DamagedOtau(string ip, int tcpPort)
        {
            Ip = ip;
            TcpPort = tcpPort;
            RebootStarted = DateTime.Now;
            RebootAttempts = 0;
        }
    }
}
