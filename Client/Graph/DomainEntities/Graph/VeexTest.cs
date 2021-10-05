﻿using System;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    [Serializable]
    public class VeexTest
    {
        public Guid TestId { get; set; }
        public Guid TraceId { get; set; }
        public BaseRefType BaseRefType { get; set; }

        public bool IsOnBop { get; set; }
        public Guid OtauId { get; set; }

        public DateTime CreationTimestamp { get; set; }

    }
}