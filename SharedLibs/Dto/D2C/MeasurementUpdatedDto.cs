using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class MeasurementUpdatedDto
    {
        [DataMember]
       public Measurement UpdatedMeasurement { get; set; }


        [DataMember]
        public ReturnCode ReturnCode { get; set; }
        [DataMember]
        public string ExceptionMessage { get; set; }
    }
}