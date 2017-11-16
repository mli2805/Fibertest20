using System;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Client
{
    public class OpticalEventVm
    {
        public int Nomer { get; set; }
        public DateTime EventTimestamp { get; set; }
        public string RtuTitle { get; set; }
        public string TraceTitle { get; set; }
        public FiberState TraceState { get; set; }
        public EventStatus EventStatus { get; set; }
        public DateTime StatusTimestamp { get; set; }
        public string StatusUsername { get; set; }
    }
}