using System;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class RtuConnectionCheckedDto
    {
        [DataMember]
        public string ClientIp { get; set; }

        [DataMember]
        public Guid RtuId { get; set; }

        [DataMember]
        public bool IsConnectionSuccessfull { get; set; }
        [DataMember]
        public NetAddress NetAddress { get; set; }

        [DataMember]
        public bool IsPingSuccessful { get; set; } // check if rtu service connection failed
    }
}