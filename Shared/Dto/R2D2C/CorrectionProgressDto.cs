using System;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class CorrectionProgressDto
    {
        [DataMember]
        public Guid BatchId { get; set; }
        [DataMember]
        public ReturnCode ReturnCode { get; set; }
        [DataMember]
        public string ErrorMessage { get; set; }

        [DataMember]
        public Guid TraceId { get; set; } // if message about trace
        [DataMember]
        public int AllTracesInvolved { get; set; }
        [DataMember]
        public int TracesCorrected { get; set; }
    }
}