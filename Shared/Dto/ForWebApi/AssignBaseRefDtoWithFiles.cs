using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    public class AssignBaseRefDtoWithFiles
    {
        [DataMember]
        public Guid RtuId { get; set; }
        [DataMember]
        public RtuMaker RtuMaker { get; set; }
        [DataMember]
        public string OtdrId { get; set; } //  in VeEX RTU main OTDR has its own ID

        [DataMember]
        public Guid TraceId { get; set; }

        [DataMember]
        public OtauPortDto OtauPortDto { get; set; } // could be null if trace isn't attached to port yet

        [DataMember]
        public List<BaseRefFile> BaseRefs { get; set; }

    }
}