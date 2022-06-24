using System;

namespace Iit.Fibertest.Dto
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
                Main = Main.Clone(),
                HasReserveAddress = HasReserveAddress,
                Reserve = Reserve?.Clone(),
            };
        }
    }
}