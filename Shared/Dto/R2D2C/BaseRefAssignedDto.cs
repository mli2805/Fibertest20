using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class BaseRefAssignedDto
    {
        [DataMember]
        public ReturnCode ReturnCode { get; set; }

        [DataMember]
        public BaseRefType BaseRefType { get; set; }
        [DataMember]
        public string ErrorMessage { get; set; }

        [DataMember]
        public int Landmarks { get; set; }
        [DataMember]
        public int Nodes { get; set; }
        [DataMember]
        public int Equipments { get; set; }
        [DataMember]
        public string WaveLength { get; set; }
    }
}