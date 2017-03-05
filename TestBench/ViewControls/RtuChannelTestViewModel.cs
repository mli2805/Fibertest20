using Iit.Fibertest.Graph;

namespace Iit.Fibertest.TestBench
{
    public class RtuChannelTestViewModel
    {
        public NetAddressInputViewModel NetAddressInputViewModel { get; set; }

        public RtuChannelTestViewModel(NetAddress netAddress)
        {
            NetAddressInputViewModel = new NetAddressInputViewModel(netAddress);
        }

        public void Test()
        {
            
        }
    }
}
