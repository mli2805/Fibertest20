﻿using System;
using System.Collections.Generic;

namespace Iit.Fibertest.Graph.Commands
{
    public class AssignBaseRef
    {
        public Guid TraceId { get; set; }

        public Dictionary<BaseRefType, Guid> Ids { get; set; } = new Dictionary<BaseRefType, Guid>();
        public Dictionary<Guid, byte[]> Contents { get; set; } = new Dictionary<Guid, byte[]>();

    }
}
