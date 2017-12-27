using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class BopNetworkEventsList
    {
        [DataMember]
        public List<BopNetworkEvent> ActualEvents { get; set; }

        [DataMember]
        public List<BopNetworkEvent> PageOfLastEvents { get; set; }
    }
}