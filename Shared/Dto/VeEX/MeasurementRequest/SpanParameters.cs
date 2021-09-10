// ReSharper disable InconsistentNaming
namespace Iit.Fibertest.Dto
{
    public class SpanParameters
    {
        public int beginningEventIndex { get; set; }
        public int endEventIndex { get; set; }
        public bool includeBeginningEventLoss { get; set; }
        public bool includeEndEventLoss { get; set; }
    }
}