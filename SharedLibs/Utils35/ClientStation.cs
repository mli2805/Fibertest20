using System;
using Dto;

namespace Iit.Fibertest.UtilsLib
{
    public class ClientStation : ICloneable
    {
        public Guid Id { get; set; }
        public DoubleAddressWithLastConnectionCheck PcAddresses { get; set; }

        public object Clone()
        {
            return new ClientStation()
            {
                Id = Id,
                PcAddresses = (DoubleAddressWithLastConnectionCheck) PcAddresses.Clone()
            };
        }
    }
}