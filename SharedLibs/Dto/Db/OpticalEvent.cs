using System;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    public class OpticalEvent
    {
        public int Id { get; set; }
        public DateTime EventTimestamp { get; set; }
        public Guid RtuId { get; set; }
        public Guid TraceId { get; set; }
        public BaseRefType BaseRefType { get; set; }
        public FiberState TraceState { get; set; }
        public EventStatus EventStatus { get; set; }
        public DateTime StatusTimestamp { get; set; }
        public int StatusUserId { get; set; }
        public string Comment { get; set; }
    }
}