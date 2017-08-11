using System;

namespace Iit.Fibertest.Utils35
{
    public class RtuStation
    {
        public Guid Id { get; set; }

        public DoubleAddressWithLastConnectioncheck Addresses { get; set; } = new DoubleAddressWithLastConnectioncheck();
    }
}