using Iit.Fibertest.Graph;

namespace Iit.Fibertest.TestBench
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
