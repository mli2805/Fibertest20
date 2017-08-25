using System;
using Dto;

namespace Iit.Fibertest.UtilsLib
{
    public class RtuStation
    {
        public Guid Id { get; set; }

        public DoubleAddressWithLastConnectionCheck Addresses { get; set; } = new DoubleAddressWithLastConnectionCheck();

        public override string ToString()
        {
            return Id.ToString().Substring(0, 6);
        }
    }
}