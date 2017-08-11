using System;
using Dto;

namespace Iit.Fibertest.Utils35
{
    public class RtuStation
    {
        public Guid Id { get; set; }

        public DoubleAddressWithLastConnectionCheck Addresses { get; set; } = new DoubleAddressWithLastConnectionCheck();
    }
}