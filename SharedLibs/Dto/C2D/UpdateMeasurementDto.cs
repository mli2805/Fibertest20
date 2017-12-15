using System;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class UpdateMeasurementDto
    {
        [DataMember]
        public Guid ClientId { get; set; }

        [DataMember]
        public int SorFileId { get; set; }
        [DataMember]
        public EventStatus EventStatus { get; set; }
        [DataMember]
        public string StatusChangedByUser { get; set; }
        [DataMember]
        public string Comment { get; set; }
    }
}