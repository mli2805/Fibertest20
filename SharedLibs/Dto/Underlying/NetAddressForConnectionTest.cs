namespace Iit.Fibertest.Dto
{
    public class NetAddressForConnectionTest
    {
        public NetAddress Address { get; set; }
        public bool IsRtuAddress { get; set; }

        public NetAddressForConnectionTest(NetAddress address, bool isRtuAddress)
        {
            Address = address;
            IsRtuAddress = isRtuAddress;
        }
    }
}