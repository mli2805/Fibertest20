using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class NetworkEventsList
    {
        [DataMember]
        public List<NetworkEvent> Events { get; set; }
    }
}