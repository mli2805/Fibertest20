using System.Collections.Generic;

namespace Iit.Fibertest.WpfCommonViews
{
    public class EventsContent
    {
        public Dictionary<int, string[]> Table { get; set; } = new Dictionary<int, string[]>();
        public bool IsFailed { get; set; }
    }
}