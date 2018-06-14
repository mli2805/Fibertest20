using System;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class DoClientMeasurementDto
    {
        [DataMember]
        public Guid ClientId { get; set; }

        [DataMember]
        public Guid RtuId { get; set; }

        [DataMember]
        public SelectedMeasParams SelectedMeasParams { get; set; }

        [DataMember]
        public OtauPortDto OtauPortDto { get; set; }
    }
}