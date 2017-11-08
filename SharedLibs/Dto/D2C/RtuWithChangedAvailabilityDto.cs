using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    
    [DataContract]
    public class ListOfRtuWithChangedAvailabilityDto
    {
        [DataMember]
        public List<RtuWithChannelChanges> List { get; set; }
    }
}