using System;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    [Serializable]
    public class VeexTest
    {
        public Guid TestId { get; set; }
        public Guid TraceId { get; set; }
        public BaseRefType BaseRefType { get; set; }

    }
}