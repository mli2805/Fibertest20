using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class TraceStateDto
    {
        [DataMember]
        public Measurement LastMeasurement { get; set; }

        [DataMember]
        public OpticalEvent CorrespondentEvent { get; set; }

        [DataMember]
        public byte[] SorBytes { get; set; }
    }
}