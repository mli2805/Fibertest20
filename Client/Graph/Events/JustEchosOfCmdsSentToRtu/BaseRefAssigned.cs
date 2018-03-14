using System;
using System.Collections.Generic;

namespace Iit.Fibertest.Graph
{
    [Serializable]
    public class BaseRefAssigned
    {
        public Guid TraceId { get; set; }

        public List<BaseRef> BaseRefs { get; set; } = new List<BaseRef>();

    }
}
