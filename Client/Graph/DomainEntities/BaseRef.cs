using System;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    public class BaseRef
    {
        public Guid Id { get; set; }

        public Guid TraceId { get; set; }
        public BaseRefType BaseRefType { get; set; }
        public string UserName { get; set; }
        public DateTime SaveTimestamp { get; set; }
        public TimeSpan Duration { get; set; }

        public int SorFileId { get; set; }
    }
}
