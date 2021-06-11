using System;
using System.Collections.Generic;

namespace Iit.Fibertest.Dto
{
    public class OtdrEngine
    {
        public string IitOtdr { get; set; }
    }

    public class Other
    {
        public string OsInfo { get; set; }
        public string PlatformFirmware { get; set; }
    }

    public class Components
    {
        public string Api { get; set; }
        public string Core { get; set; }
        public string HttpServer { get; set; }
        public OtdrEngine OtdrEngine { get; set; }
        public Other Other { get; set; }
    }

    public class Platform
    {
        public List<string> EnabledOptions { get; set; }
        public string FirmwareVersion { get; set; }
        public string ModuleFirmwareVersion { get; set; }
        public string Name { get; set; }
        public string SerialNumber { get; set; }
    }

    public class Info
    {
        public Components Components { get; set; }
        public DateTime DateTime { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public Platform Platform { get; set; }
    }
}
