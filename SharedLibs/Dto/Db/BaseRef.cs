using System;

namespace Iit.Fibertest.Dto
{
    public class BaseRef
    {
        public int Id { get; set; }
        public Guid BaseRefId { get; set; }
        public Guid TraceId { get; set; }
        public BaseRefType BaseRefType { get; set; }
        public int UserId { get; set; }
        public DateTime SaveTimestamp { get; set; }
        public byte[] SorBytes { get; set; }
    }
}
