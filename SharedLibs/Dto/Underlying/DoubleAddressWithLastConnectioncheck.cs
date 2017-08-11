using System;

namespace Dto
{
    public class DoubleAddressWithLastConnectionCheck : ICloneable
    {
        public NetAddress Main { get; set; }
        public DateTime LastConnectionOnMain { get; set; }

        public bool HasReserveAddress { get; set; }
        public NetAddress Reserve { get; set; }
        public DateTime LastConnectionOnReserve { get; set; }

        public object Clone()
        {
            return new DoubleAddressWithLastConnectionCheck()
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