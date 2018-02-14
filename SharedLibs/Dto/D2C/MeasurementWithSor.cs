using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class MeasurementWithSor
    {
        [DataMember]
        public Measurement Measurement { get; set; }

        [DataMember]
        public byte[] SorBytes { get; set; }
    }
}