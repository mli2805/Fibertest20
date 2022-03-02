using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class TraceStateFilter
    {
        public bool IsOn { get; set; }
        public FiberState TraceState { get; set; }

        public TraceStateFilter() { IsOn = false; }

        public TraceStateFilter(FiberState traceState)
        {
            IsOn = true;
            TraceState = traceState;
        }

        public override string ToString()
        {
            return IsOn ? TraceState.ToLocalizedString() : Resources.SID__no_filter_;
        }
    }
}