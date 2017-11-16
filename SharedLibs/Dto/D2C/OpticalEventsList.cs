using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class OpticalEventsList
    {
        [DataMember]
        public List<OpticalEvent> Events { get; set; }
    }
}
