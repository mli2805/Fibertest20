using System;
using System.Collections.Generic;

namespace Iit.Fibertest.Dto
{
    public class NetworkEventDto
    {
        public int EventId;
        public DateTime EventRegistrationTimestamp;
        public string RtuTitle;

        public bool IsRtuAvailable;
        public ChannelEvent MainChannelEvent;
        public ChannelEvent ReserveChannelEvent;
    }

    public class NetworkEventsRequestedDto
    {
        public int FullCount;
        public List<NetworkEventDto> EventPortion;
    }
}
