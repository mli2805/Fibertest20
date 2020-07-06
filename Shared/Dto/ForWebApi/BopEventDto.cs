using System;
using System.Collections.Generic;

namespace Iit.Fibertest.Dto
{
    public class BopEventDto
    {
        public int EventId;
        public DateTime EventRegistrationTimestamp;
        public string BopAddress;
        public Guid RtuId;
        public string RtuTitle;

        public bool BopState;
    }

    public class BopEventsRequestedDto
    {
        public int FullCount;
        public List<BopEventDto> EventPortion;
    }
}
