using System.Collections.Generic;

namespace Iit.Fibertest.WpfCommonViews
{
    public class EventContent
    {
        public Dictionary<int, string[]> Table { get; set; } = new Dictionary<int, string[]>();
        public bool IsFailed { get; set; }
        public string FirstProblemLocation { get; set; }
    }
}