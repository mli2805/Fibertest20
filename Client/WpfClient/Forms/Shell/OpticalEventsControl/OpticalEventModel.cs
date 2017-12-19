using System;
using System.Windows.Media;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Client
{
    public class OpticalEventModel
    {
        public int Nomer { get; set; }
        public DateTime MeasurementTimestamp { get; set; }
        public DateTime EventRegistrationTimestamp { get; set; }
        public string RtuTitle { get; set; }
        public Guid RtuId { get; set; }

        public Guid TraceId { get; set; }
        public string TraceTitle { get; set; }

        public BaseRefType BaseRefType { get; set; }
        public FiberState TraceState { get; set; }


        public Brush BaseRefTypeBrush =>
            TraceState == FiberState.Ok
                ? Brushes.White
                : BaseRefType == BaseRefType.Fast
                    ? Brushes.Yellow
                    : TraceState.GetBrush(isForeground: false);

        public string TraceStateOnScreen => BaseRefType == BaseRefType.Fast && TraceState != FiberState.Ok
            ? FiberState.Suspicion.ToLocalizedString() 
            : TraceState.ToLocalizedString();


        public EventStatus EventStatus { get; set; }

        public Brush EventStatusBrush => EventStatus == EventStatus.Confirmed ? Brushes.Red : Brushes.White;
        public string EventStatusInTable => EventStatus.GetLocalizedString();

        public string StatusChangedTimestamp { get; set; }
        public string StatusChangedByUser { get; set; }
        public string Comment { get; set; }


        public int SorFileId { get; set; }
    }
}