using System;
using System.Windows.Media;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Client
{
    public class OpticalEventVm
    {
        public int Nomer { get; set; }
        public DateTime EventRegistrationTimestamp { get; set; }
        public string RtuTitle { get; set; }
        public string TraceTitle { get; set; }
        public Brush BaseRefTypeBrush { get; set; }

        public FiberState TraceState { get; set; }
        public string TraceStateInTable => TraceState.GetLocalizedString();

        public EventStatus EventStatus { get; set; }
        public Brush EventStatusBrush { get; set; }
        public string EventStatusInTable => EventStatus.GetLocalizedString();

        public string StatusChangedTimestamp { get; set; }
        public string StatusChangedByUser { get; set; }

        public string Comment { get; set; }
        public int SorFileId { get; set; }
    }
}