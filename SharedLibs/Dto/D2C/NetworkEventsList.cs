using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class NetworkEventsList
    {
        [DataMember]
        public List<NetworkEvent> ActualEvents { get; set; }

        [DataMember]
        public List<NetworkEvent> PageOfLastEvents { get; set; }
    }
}