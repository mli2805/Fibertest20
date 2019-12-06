using System;
using Caliburn.Micro;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Client
{
    public class NetAddressInputViewModel : PropertyChangedBase
    {
        public Ip4InputViewModel Ip4InputViewModel { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }

        public bool IsAddressSetAsIp { get; set; }

        public bool IsAddressSetAsName => !IsAddressSetAsIp;
        public bool IsEditEnabled { get; set; }

        public string PortStr => Port > 0 ? Port.ToString() : "";

        public NetAddressInputViewModel(NetAddress netAddress, bool isEditEnabled)
        {
            Ip4InputViewModel = new Ip4InputViewModel(netAddress.Ip4Address);
            Host = netAddress.HostName;
            Port = netAddress.Port;
            IsAddressSetAsIp = netAddress.IsAddressSetAsIp;
            IsEditEnabled = isEditEnabled;
        }

        public NetAddress GetNetAddress()
        {
            return new NetAddress()
            {
                HostName = Host,
                Ip4Address = Ip4InputViewModel.GetString(),
                IsAddressSetAsIp = IsAddressSetAsIp,
                Port = PortStr == "" ? -1 : Int32.Parse(PortStr),
            };
        }
    }
}
