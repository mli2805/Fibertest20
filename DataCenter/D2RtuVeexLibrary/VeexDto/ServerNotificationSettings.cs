using System.Collections.Generic;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public class ServerNotificationSettings
    {
        public string state { get; set; }
        public List<string> eventTypes { get; set; }
        public string url { get; set; }
    }
}