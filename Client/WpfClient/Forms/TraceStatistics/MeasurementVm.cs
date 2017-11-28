using System;
using System.Windows.Media;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Client
{
    public class MeasurementVm
    {
        public int SorFileId { get; set; }
        public bool IsOpticalEvent { get; set; }
        public BaseRefType BaseRefType { get; set; }
        public string BaseRefTypeString => BaseRefType.GetLocalizedString();
        public DateTime Timestamp { get; set; }
        public FiberState TraceState { get; set; }
        public Brush TraceStateBrush =>
            TraceState == FiberState.Ok
                ? Brushes.White
                : BaseRefType == BaseRefType.Fast
                    ? Brushes.Yellow
                    : TraceState.GetBrush(isForeground: false);
        public string TraceStateOnScreen => BaseRefType == BaseRefType.Fast && TraceState != FiberState.Ok
            ? FiberState.Suspicion.GetLocalizedString()
            : TraceState.GetLocalizedString();
    }
}
