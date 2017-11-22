using System;

namespace Iit.Fibertest.Dto
{
    public class OpticalEvent
    {
        public int Id { get; set; }
        public DateTime EventRegistrationTimestamp { get; set; }
        public Guid RtuId { get; set; }
        public Guid TraceId { get; set; }
        public BaseRefType BaseRefType { get; set; }
        public FiberState TraceState { get; set; }
        public EventStatus EventStatus { get; set; }
        public DateTime StatusChangedTimestamp { get; set; }
        public string StatusChangedByUser { get; set; }
        public string Comment { get; set; }
        public int SorFileId { get; set; }
    }

    public static class OpticalEventExtension
    {
        public static bool IsStatusAcceptable(this OpticalEvent opticalEvent)
        {
            return opticalEvent.TraceState != FiberState.Ok && opticalEvent.BaseRefType != BaseRefType.Fast;
        }
    }
}