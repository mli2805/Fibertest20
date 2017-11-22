using System;
using System.Windows.Media;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Client
{
    public class MeasurementVm
    {
        public int Nomer { get; set; }
        public Guid MeasurementId { get; set; }
        public BaseRefType BaseRefType { get; set; }
        public string BaseRefTypeString => BaseRefType.GetLocalizedString();
        public DateTime Timestamp { get; set; }
        public FiberState TraceState { get; set; }
        public Brush TraceStateBrush => TraceState.GetBrush(isForeground:false);
        public string TraceStateInTable => TraceState.GetLocalizedString();
    }
}
