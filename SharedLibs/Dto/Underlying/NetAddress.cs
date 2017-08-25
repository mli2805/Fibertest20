using System;

namespace Dto
{
    [Serializable]
    public class NetAddress : ICloneable
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

        public NetAddress(string ip, TcpPorts port)
        {
            Ip4Address = ip;
            HostName = "";
            Port = (int)port;
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

        public string ToStringASpace => IsAddressSetAsIp ? $@"{Ip4Address} : {Port}" : $@"{HostName} : {Port}";
        public string ToStringA() => IsAddressSetAsIp ? $@"{Ip4Address}:{Port}" : $@"{HostName} : {Port}";
        public string ToStringB() => Port == 11834 ? $@"{Ip4Address}(1)" : $@"{Ip4Address}(2)";

        public bool HasValidIp4Address()
        {
            var parts = Ip4Address.Split('.');
            if (parts.Length != 4)
                return false;

            for (var i = 0; i < 3; i++)
            {
                int part;
                if (!int.TryParse(parts[i], out part))
                    return false;
                if (part < 0 || part > 255)
                    return false;
            }

            return true;
        }

        public bool HasValidTcpPort()
        {
            return Port > 0 && Port < 65356;
        }

        public object Clone()
        {
            return new NetAddress()
            {
                HostName = (string)HostName.Clone(),
                Ip4Address = (string)Ip4Address.Clone(),
                Port = Port,
                IsAddressSetAsIp = IsAddressSetAsIp,
            };
        }
    }
}