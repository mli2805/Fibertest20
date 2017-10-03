using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class BaseRefDto
    {
        [DataMember]
        public BaseRefType BaseRefType { get; set; }

        [DataMember]
        public byte[] SorBytes { get; set; }
    }
}