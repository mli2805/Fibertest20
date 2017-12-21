using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class MeasurementsList
    {
        [DataMember]
        public List<Measurement> ActualMeasurements { get; set; }

        [DataMember]
        public List<Measurement> PageOfLastMeasurements { get; set; }
    }
}
