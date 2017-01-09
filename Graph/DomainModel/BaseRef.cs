﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iit.Fibertest.Graph
{
    public class BaseRef
    {
        public Guid Id { get; set; }
        public Guid TraceId { get; set; }
        public BaseRefType Type { get; set; }
        public byte[] Content { get; set; }
    }
}
