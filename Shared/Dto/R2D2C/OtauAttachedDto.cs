using System;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class OtauAttachedDto : RequestAnswer
    {
        [DataMember]
        public Guid OtauId { get; set; }

        [DataMember]
        public Guid RtuId { get; set; }

        [DataMember]
        public bool IsAttached { get; set; }

        //[DataMember]
        //public ReturnCode ReturnCode { get; set; }
        //[DataMember]
        //public string ErrorMessage { get; set; }

        [DataMember]
        public string Serial { get; set; }

        [DataMember]
        public int PortCount { get; set; }

        public OtauAttachedDto()
        {
        }

        public OtauAttachedDto(ReturnCode returnCode) : base(returnCode)
        {
        }
    }
}