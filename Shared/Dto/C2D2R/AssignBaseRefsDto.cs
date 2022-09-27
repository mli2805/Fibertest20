using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class AssignBaseRefsDto
    {
        [DataMember]
        public string Username { get; set; }
        [DataMember]
        public string ClientIp { get; set; }

        [DataMember]
        public string ConnectionId { get; set; }

        [DataMember]
        public Guid RtuId { get; set; } 
        [DataMember]
        public RtuMaker RtuMaker { get; set; }

        [DataMember]
        public string OtdrId { get; set; } //  in VeEX RTU main OTDR has its own ID
        
        [DataMember]
        public Guid TraceId { get; set; }

        [DataMember]
        public bool IsAutoBase { get; set; }

        [DataMember]
        public OtauPortDto OtauPortDto { get; set; } // could be null if trace isn't attached to port yet

        [DataMember]
        public OtauPortDto MainOtauPortDto { get; set; } // optional, filled in if trace attached to the child otau

        [DataMember]
        public List<BaseRefDto> BaseRefs { get; set; }

        [DataMember]
        public List<int> DeleteOldSorFileIds { get; set; } = new List<int>();
    }
}