using System.Windows.Media;
using Caliburn.Micro;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Client
{
    public class TraceStateViewModel : Screen
    {
        public string TraceTitle { get; set; }
        public string RtuTitle { get; set; }
        public string PortTitle { get; set; }
        public FiberState TraceState { get; set; }
        public BaseRefType BaseRefType { get; set; }

        //public Brush TraceStateBrush => BaseRefType == BaseRefType.Fast && TraceState != FiberState.Ok ? 
        public Brush TraceStateBrush => TraceState.GetBrush(isForeground: true);
        public void Initialize(int sorFileId)
        {
            TraceTitle = "bla!";
            TraceState = FiberState.Suspicion;
        }
        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Trace state";
        }
    }
}
