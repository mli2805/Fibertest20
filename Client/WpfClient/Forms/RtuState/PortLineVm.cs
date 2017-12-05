using System;
using System.Threading;
using System.Windows.Media;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Client
{
    public class PortLineVm
    {
        public string Number { get; set; }
        public Guid TraceId { get; set; } = Guid.Empty;
        public string TraceTitle { get; set; }

        public FiberState TraceState { get; set; }
        public string TraceStateOnScreen => TraceId == Guid.Empty ? "" : TraceState.ToLocalizedString();
        public Brush TraceStateBrush => TraceState.GetBrush(false);

        public DateTime? Timestamp { get; set; }

        public string TimestampOnScreen => Timestamp?.ToString(Thread.CurrentThread.CurrentUICulture) ?? "";
    }
}