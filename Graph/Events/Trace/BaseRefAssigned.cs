﻿using System;
using System.Collections.Generic;
using Iit.Fibertest.Utils35;

namespace Iit.Fibertest.Graph
{
    [Serializable]
    public class BaseRefAssigned
    {
        public Guid TraceId { get; set; }

        public Dictionary<BaseRefType, Guid> Ids { get; set; } = new Dictionary<BaseRefType, Guid>();
        public Dictionary<Guid, byte[]> Contents { get; set; } = new Dictionary<Guid, byte[]>();
    }
}
