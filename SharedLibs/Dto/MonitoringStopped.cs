using System;
using System.Runtime.Serialization;

namespace Dto
{
    [DataContract]
    public class MonitoringStopped
    {
        [DataMember]
        public Guid RtuId { get; set; }

        [DataMember]
        public bool IsSuccessful { get; set; }
    }
}