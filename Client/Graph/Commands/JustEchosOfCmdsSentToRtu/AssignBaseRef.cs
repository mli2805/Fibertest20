using System;
using System.Collections.Generic;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    public class AssignBaseRef
    {
        public Guid TraceId { get; set; }

        public List<BaseRefDto> BaseRefs { get; set; } = new List<BaseRefDto>();
    }
}
