using System;
using System.Collections.Generic;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    [Serializable]
    public class BaseRefAssigned
    {
        public Guid TraceId { get; set; }

        public List<BaseRefDto> BaseRefs { get; set; } = new List<BaseRefDto>();

    }
}
