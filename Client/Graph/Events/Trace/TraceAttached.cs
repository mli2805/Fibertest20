using System;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    [Serializable]
    public class TraceAttached
    {
        public int Port { get; set; }
        public Guid TraceId { get; set; }
        public OtauPortDto OtauPortDto { get; set; }

    }
}
