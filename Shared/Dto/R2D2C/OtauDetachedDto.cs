using System;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class OtauDetachedDto
    {
        [DataMember]
        public Guid OtauId { get; set; }

        [DataMember]
        public Guid RtuId { get; set; }

        [DataMember]
        public bool IsDetached { get; set; }

        [DataMember]
        public ReturnCode ReturnCode { get; set; }
        [DataMember]
        public string ErrorMessage { get; set; }

        public OtauDetachedDto()
        {
        }

        public OtauDetachedDto(ReturnCode returnCode)
        {
            ReturnCode = returnCode;
        }
    }
}