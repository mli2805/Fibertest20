using System;
using System.IO;
using Iit.Fibertest.Utils35;

namespace Iit.Fibertest.DirectCharonLibrary
{
    public enum PortMeasResult
    {
        Unknown            =0, // just started , no previous measurements
        Ok                 =1,
        BrokenByFast       =2,
        BrokenByPrecise    =3,
    }
    public class ExtendedPort
    {
        public NetAddress NetAddress { get; set; }
        public int Port { get; set; }

        public DateTime LastPreciseMadeTimestamp { get; set; }
        public DateTime LastPreciseSavedTimestamp { get; set; }
        public DateTime LastFastSavedTimestamp { get; set; }

        public PortMeasResult State { get; set; }
        public bool IsBreakdownCloserThen20Km { get; set; }

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

        public bool HasAdditionalBase()
        {
            var basefile = $@"..\PortData\{GetFolderName()}\{BaseRefType.Additional.ToFileName()}";
            return File.Exists(basefile);
        }
    }
}
