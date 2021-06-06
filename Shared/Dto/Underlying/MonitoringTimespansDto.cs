using System;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    public class MonitoringTimespansDto
    {
        [DataMember]
        public TimeSpan PreciseMeas { get; set; }

        [DataMember]
        public TimeSpan PreciseSave { get; set; }

        [DataMember]
        public TimeSpan FastSave { get; set; }
    }
}