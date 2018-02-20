using System;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class ClientRegisteredDto
    {
        [DataMember]
        public ReturnCode ReturnCode { get; set; }

        [DataMember]
        public string ExceptionMessage { get; set; }

        [DataMember]
        public Guid UserId { get; set; }

        [DataMember]
        public Role Role { get; set; }

        [DataMember]
        public Guid GraphDbVersionId { get; set; }
    }
}