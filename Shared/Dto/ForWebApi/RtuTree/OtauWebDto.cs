using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class OtauWebDto : ChildDto
    {
        [DataMember] public Guid OtauId;
        [DataMember] public Guid RtuId;
        [DataMember] public NetAddress OtauNetAddress;
        [DataMember] public bool IsOk;
        [DataMember] public string Serial;

        [DataMember] public List<ChildDto> Children = new List<ChildDto>();

        public OtauWebDto(ChildType childType) : base(childType)
        {
        }
    }
}