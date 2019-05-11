using System;

namespace Iit.Fibertest.Dto
{
    public class Snapshot
    {
        public int Id { get; set; }
        public Guid AggregateId { get; set; }
        public int LastEventNumber { get; set; }
        public DateTime LastEventDate { get; set; }
        public byte[] Payload { get; set; }
    }
}