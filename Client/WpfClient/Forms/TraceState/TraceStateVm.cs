using System.Windows;
using System.Windows.Media;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Client
{
    public class TraceStateVm
    {
        public string TraceTitle { get; set; }
        public string RtuTitle { get; set; }
        public string PortTitle { get; set; }

        public FiberState TraceState { get; set; }
        public BaseRefType BaseRefType { get; set; }

        //public Brush TraceStateBrush => BaseRefType == BaseRefType.Fast && TraceState != FiberState.Ok ? 
        public Brush TraceStateBrush => TraceState.GetBrush(isForeground: true);

        public int OpticalEventId { get; set; }
        public EventStatus EventStatus { get; set; } = EventStatus.NotAnAccident;
        public string OpticalEventComment { get; set; }


        public byte[] SorBytes { get; set; }

        public Visibility OpticalEventPanelVisibility
            => EventStatus == EventStatus.NotAnAccident ? Visibility.Collapsed : Visibility.Visible;



    }
}
