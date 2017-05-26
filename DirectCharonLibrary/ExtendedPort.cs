using Iit.Fibertest.Utils35;

namespace Iit.Fibertest.DirectCharonLibrary
{
    public class ExtendedPort
    {
        public NetAddress NetAddress { get; set; }
        public int Port { get; set; }

        public ExtendedPort(NetAddress netAddress, int opticalPort)
        {
            NetAddress = netAddress;
            Port = opticalPort;
        }

        public string GetFolderName()
        {
            return $"{NetAddress.Ip4Address}t{NetAddress.Port}p{Port}";
        }

        public string ToStringA()
        {
            return $"{Port} on {NetAddress.ToStringA()}";
        }
    }
}
