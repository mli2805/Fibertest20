using System;

namespace Iit.Fibertest.Utils35
{
    public class DoubleAddressWithLastConnectioncheck : ICloneable
    {
        public NetAddress Main { get; set; }
        public DateTime LastConnectionOnMain { get; set; }

        public bool HasReserveAddress { get; set; }
        public NetAddress Reserve { get; set; }
        public DateTime LastConnectionOnReserve { get; set; }

        public object Clone()
        {
            return new DoubleAddressWithLastConnectioncheck()
            {
                Main = (NetAddress)Main.Clone(),
                LastConnectionOnMain = LastConnectionOnMain,
                HasReserveAddress = HasReserveAddress,
                Reserve = (NetAddress)Reserve.Clone(),
                LastConnectionOnReserve = LastConnectionOnReserve,
            };
        }

    }
}