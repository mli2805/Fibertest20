using Iit.Fibertest.Graph;

namespace Iit.Fibertest.TestBench
{
    public class NetAddressInputViewModel
    {
        public Ip4InputViewModel Ip4InputViewModel { get; set; }
        public string Domain { get; set; }
        public int Port { get; set; }

        public NetAddressInputViewModel(NetAddress netAddress)
        {
            Ip4InputViewModel = new Ip4InputViewModel(netAddress.Ip4Address);
            Domain = netAddress.HostName;
            Port = netAddress.Port;
        }
    }
}
