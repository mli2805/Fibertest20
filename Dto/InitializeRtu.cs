using System;
using System.Runtime.Serialization;

namespace Dto
{
    [DataContract]
    public class InitializeRtu
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public string IpAddress { get; set; }
    }
}
