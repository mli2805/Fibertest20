using System;
using System.Collections.Generic;
// ReSharper disable InconsistentNaming

namespace Iit.Fibertest.Dto
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
        public OtdrEngine otdrEngine { get; set; } = new OtdrEngine();
        public Other other { get; set; }
    }

    public class IpSettings
    {
        public string gateway { get; set; }
        public string ip { get; set; }
        public bool isIpV6Supported { get; set; }
        public string netmask { get; set; }
    }

    public class NetworkSettings
    {
        public List<object> dns { get; set; }
        public string dnsChangeable { get; set; }
        public IpSettings ipSettings { get; set; }
        public string ipSettingsChangeable { get; set; }
    }

    public class Platform
    {
        public List<string> enabledOptions { get; set; }
        public string firmwareVersion { get; set; }
        public string moduleFirmwareVersion { get; set; }
        public string moduleSerialNumber { get; set; }
        public string name { get; set; }
        public string serialNumber { get; set; }
    }

    public class VeexPlatformInfo
    {
        public bool apiAuthEnabled { get; set; }
        public string apiAuthEnabledChangeable { get; set; }
        public Components components { get; set; } = new Components();
        public string cpu { get; set; }
        public DateTime dateTime { get; set; }
        public string dateTimeChangeable { get; set; }
        public string id { get; set; }
        public List<string> langs { get; set; }
        public string name { get; set; }
        public string nameChangeable { get; set; }
        public NetworkSettings networkSettings { get; set; }
        public bool onTemporaryStorage { get; set; }
        public Platform platform { get; set; } = new Platform();
        public string rebootable { get; set; }
    }
}
