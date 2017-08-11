using System;

namespace Iit.Fibertest.Utils35
{
    public class ClientStation : ICloneable
    {
        public DoubleAddressWithLastConnectioncheck Addresses { get; set; }

        public object Clone()
        {
            return new ClientStation()
            {
                Addresses = (DoubleAddressWithLastConnectioncheck) Addresses.Clone()
            };
        }
    }
}