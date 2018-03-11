using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Client
{
    public class NetAddressInputViewModel
    {
        public Ip4InputViewModel Ip4InputViewModel { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }

        public bool IsAddressSetAsIp { get; set; }

        public bool IsAddressSetAsName => !IsAddressSetAsIp;

        public NetAddressInputViewModel(NetAddress netAddress)
        {
            Ip4InputViewModel = new Ip4InputViewModel(netAddress.Ip4Address);
            Host = netAddress.HostName;
            Port = netAddress.Port;
            IsAddressSetAsIp = netAddress.IsAddressSetAsIp;
        }

        public NetAddress GetNetAddress()
        {
            return new NetAddress()
            {
                HostName = Host,
                Ip4Address = Ip4InputViewModel.GetString(),
                IsAddressSetAsIp = IsAddressSetAsIp,
                Port = Port
            };
        }
    }
}
