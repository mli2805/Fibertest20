using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class MeasurementsList
    {
        [DataMember]
        public List<Measurement> Measurements { get; set; }
    }
}
