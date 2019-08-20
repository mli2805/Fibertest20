using System;
using System.Collections.Generic;

namespace HttpLib
{
    public class OtdrEngine
    {
        public string iit_otdr { get; set; }
    }

    public class Other
    {
        public string os_info { get; set; }
        public string platform_firmware { get; set; }
    }

    public class Components
    {
        public string api { get; set; }
        public string core { get; set; }
        public string httpServer { get; set; }
        public OtdrEngine otdrEngine { get; set; }
        public Other other { get; set; }
    }

    public class Platform
    {
        public List<string> enabledOptions { get; set; }
        public string firmwareVersion { get; set; }
        public string moduleFirmwareVersion { get; set; }
        public string name { get; set; }
        public string serialNumber { get; set; }
    }

    public class Info
    {
        public Components components { get; set; }
        public DateTime dateTime { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public Platform platform { get; set; }
    }
}
