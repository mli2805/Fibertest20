using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class TraceStatistics
    {
        [DataMember]
        public List<Measurement> Measurements { get; set; }

        [DataMember]
        public List<BaseRefForStats> BaseRefs { get; set; }
    }
}