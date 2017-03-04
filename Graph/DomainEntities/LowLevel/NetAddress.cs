using System;

namespace Iit.Fibertest.Graph
{
    [Serializable]
    public class NetAddress
    {
        public string Ip4Address { get; set; } // 172.35.98.128
        public string HostName { get; set; } // domain.beltelecom.by 
        public int Port { get; set; }
    }
}