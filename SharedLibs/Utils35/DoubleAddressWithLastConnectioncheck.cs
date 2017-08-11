using System;

namespace Iit.Fibertest.Utils35
{
    public class DoubleAddressWithLastConnectioncheck
    {
        public NetAddress Main { get; set; }
        public DateTime LastConnectionOnMain { get; set; }

        public bool HasReserveAddress { get; set; }
        public NetAddress Reserve { get; set; }
        public DateTime LastConnectionOnReserve { get; set; }

    }
}