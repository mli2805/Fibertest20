using System.Runtime.Serialization;
using Dto.Enums;

namespace Dto
{
    [DataContract]
    public class AssignBaseRefDto
    {
        [DataMember]
        public string RtuIpAddress { get; set; }

        [DataMember]
        public OtauPortDto OtauPortDto { get; set; }

        [DataMember]
        public BaseRefType BaseRefType { get; set; }

        [DataMember]
        public byte[] SorBytes { get; set; }
    }
}