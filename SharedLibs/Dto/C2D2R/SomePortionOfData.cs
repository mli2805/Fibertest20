using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class SomePortionOfData
    {
        [DataMember]
        public object Something { get; set; }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public NetAddress SomeWhere { get; set; }

        [DataMember]
        public int Count { get; set; }
    }
}