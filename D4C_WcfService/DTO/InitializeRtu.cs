using System;
using System.Runtime.Serialization;

namespace D4C_WcfService
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
