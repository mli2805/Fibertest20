using System;
using Dto;

namespace Iit.Fibertest.Utils35
{
    public class ClientStation : ICloneable
    {
        public DoubleAddressWithLastConnectionCheck Addresses { get; set; }

        public object Clone()
        {
            return new ClientStation()
            {
                Addresses = (DoubleAddressWithLastConnectionCheck) Addresses.Clone()
            };
        }
    }
}