using System;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class MeasurementUpdatedDto
    {
        [DataMember]
        public int SorFileId { get; set; }
        [DataMember]
        public string StatusChangedByUser { get; set; }
        [DataMember]
        public DateTime StatusChangedTimestamp { get; set; }


        [DataMember]
        public ReturnCode ReturnCode { get; set; }
        [DataMember]
        public string ExceptionMessage { get; set; }
    }
}