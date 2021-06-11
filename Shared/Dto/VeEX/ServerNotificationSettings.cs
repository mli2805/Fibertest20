using System.Collections.Generic;

namespace Iit.Fibertest.Dto
{
    public class ServerNotificationSettings
    {
        public string State { get; set; }
        public List<string> EventTypes { get; set; }
        public string Url { get; set; }
    }
}