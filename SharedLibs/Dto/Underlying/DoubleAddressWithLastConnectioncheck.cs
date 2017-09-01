using System;

namespace Dto
{
    public class DoubleAddress : ICloneable
    {
        public NetAddress Main { get; set; } = new NetAddress();

        public bool HasReserveAddress { get; set; }
        public NetAddress Reserve { get; set; } = new NetAddress();
        public object Clone()
        {
            return new DoubleAddress()
            {
                Main = (NetAddress)Main.Clone(),
                HasReserveAddress = HasReserveAddress,
                Reserve = (NetAddress)Reserve.Clone(),
            };
        }
    }

    // on Server
    public class DoubleAddressWithLastConnectionCheck : ICloneable
    {
        public DoubleAddress DoubleAddress { get; set; }

        public DateTime LastConnectionOnMain { get; set; }
        public DateTime LastConnectionOnReserve { get; set; }

        public bool? IsLastCheckOfMainSuccessfull { get; set; }
        public bool? IsLastCheckOfReserveSuccessfull { get; set; }

        public object Clone()
        {
            return new DoubleAddressWithLastConnectionCheck()
            {
                DoubleAddress = (DoubleAddress)DoubleAddress.Clone(),
                LastConnectionOnMain = LastConnectionOnMain,
                LastConnectionOnReserve = LastConnectionOnReserve,
                IsLastCheckOfMainSuccessfull = IsLastCheckOfMainSuccessfull,
                IsLastCheckOfReserveSuccessfull = IsLastCheckOfReserveSuccessfull,
            };
        }

    }


    // on RTU
    public class DoubleAddressWithConnectionStats : ICloneable
    {
        public DoubleAddress DoubleAddress { get; set; }
        public bool? IsLastConnectionOnMainSuccessfull { get; set; }

        public bool? IsLastConnectionOnReserveSuccessfull { get; set; }

        public object Clone()
        {
            return new DoubleAddressWithConnectionStats()
            {
                DoubleAddress = (DoubleAddress)DoubleAddress.Clone(),
                IsLastConnectionOnMainSuccessfull = IsLastConnectionOnMainSuccessfull,
                IsLastConnectionOnReserveSuccessfull = IsLastConnectionOnReserveSuccessfull,
            };
        }
    }
}