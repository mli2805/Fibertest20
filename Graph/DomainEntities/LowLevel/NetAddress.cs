using System;

namespace Iit.Fibertest.Graph
{
    [Serializable]
    public class NetAddress
    {
        public string Ip4Address { get; set; } // 172.35.98.128
        public string HostName { get; set; } // domain.beltelecom.by 
        public int Port { get; set; }

        public bool IsAddressSetAsIp { get; set; }

        public NetAddress()
        {
            Ip4Address = @"0.0.0.0";
            HostName = "";
            Port = -1;
            IsAddressSetAsIp = true;
        }

        public override string ToString()
        {
            return $@"{Ip4Address}:{Port}";
        }
    }
}