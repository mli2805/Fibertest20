using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Client
{
    public class TraceStateFilter
    {
        public bool IsOn { get; set; }
        public FiberState TraceState { get; set; }

        /// <summary>
        /// таким конструктором создаетс€ ¬џключенный фильтр
        /// </summary>
        public TraceStateFilter() { IsOn = false; }

        /// <summary>
        /// а такой фильтр пропускает только "свое" значение
        /// </summary>
        /// <param name="traceState"></param>
        public TraceStateFilter(FiberState traceState)
        {
            IsOn = true;
            TraceState = traceState;
        }

        public override string ToString()
        {
            return IsOn ? TraceState.GetLocalizedString() : @"<no filter>";
        }
    }
}