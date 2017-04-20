using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class NetAddressTestViewModel
    {
        public NetAddressInputViewModel NetAddressInputViewModel { get; set; }

        public NetAddressTestViewModel(NetAddress netAddress)
        {
            NetAddressInputViewModel = new NetAddressInputViewModel(netAddress);
        }

        public void Test()
        {
            
        }
    }
}
