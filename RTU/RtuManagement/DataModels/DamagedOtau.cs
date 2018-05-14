using System;

namespace Iit.Fibertest.RtuManagement
{
    public class DamagedOtau
    {
        public string Ip { get; set; }
        public DateTime RebootStarted { get; set; }
        public int RebootAttempts { get; set; }

        public DamagedOtau(string ip)
        {
            Ip = ip;
            RebootStarted = DateTime.Now;
            RebootAttempts = 0;
        }
    }
}
