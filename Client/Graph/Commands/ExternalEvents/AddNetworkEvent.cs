using System;
using Iit.Fibertest.Dto;


namespace Iit.Fibertest.Graph
{
    public class AddNetworkEvent
    {
        public DateTime EventTimestamp { get; set; }
        public Guid RtuId { get; set; }
        public ChannelEvent OnMainChannel { get; set; }
        public ChannelEvent OnReserveChannel { get; set; }
    }
}