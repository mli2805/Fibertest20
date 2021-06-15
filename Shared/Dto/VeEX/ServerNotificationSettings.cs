using System.Collections.Generic;
// ReSharper disable InconsistentNaming

namespace Iit.Fibertest.Dto
{
    public class ServerNotificationSettings
    {
        public string state { get; set; }
        public List<string> eventTypes { get; set; }
        public string url { get; set; }
    }
}