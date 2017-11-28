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

        public Brush TraceStateBrush => TraceState.GetBrush(isForeground: true);
        public string TraceStateString => TraceState.GetLocalizedString();

        public int OpticalEventId { get; set; }
        public EventStatus EventStatus { get; set; } = EventStatus.NotAnAccident;
        public string OpticalEventComment { get; set; }


        public int SorFileId { get; set; }


        //---------
        public Visibility OpticalEventPanelVisibility
            => EventStatus == EventStatus.NotAnAccident ? Visibility.Collapsed : Visibility.Visible;

        public Visibility AccidentsPanelVisibility
            => TraceState == FiberState.Ok ? Visibility.Collapsed : Visibility.Visible;

        public bool IsAccidentButtonsEnabled => TraceState != FiberState.Ok;


    }
}
