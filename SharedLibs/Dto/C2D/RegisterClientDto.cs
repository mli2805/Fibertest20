using System;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class RegisterClientDto
    {
        [DataMember]
        public Guid ClientId { get; set; }

        [DataMember]
        public DoubleAddress Addresses { get; set; }

        [DataMember]
        public string UserName { get; set; }

        [DataMember]
        public string Password { get; set; }
    }
}