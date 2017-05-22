using System;

namespace Iit.Fibertest.Utils35
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

        public NetAddress(string ip, int port)
        {
            Ip4Address = ip;
            HostName = "";
            Port = port;
            IsAddressSetAsIp = true;
        }

        public bool Equals(NetAddress other)
        {
            if (IsAddressSetAsIp != other.IsAddressSetAsIp)
                return false;
            var isAddressEqual = IsAddressSetAsIp
                ? string.Equals(Ip4Address, other.Ip4Address)
                : string.Equals(HostName, other.HostName);
            return isAddressEqual && Port == other.Port;
        }

        public string ToStringASpace()
        {
            return $@"{Ip4Address} : {Port}";
        }
        public string ToStringA()
        {
            return $@"{Ip4Address}:{Port}";
        }

        public string ToStringB()
        {
            return Port == 11834 ? $@"{Ip4Address}(1)" : $@"{Ip4Address}(2)";
        }
    }
}