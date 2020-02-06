using System;
using System.Collections.Generic;

namespace Iit.Fibertest.Graph
{
    [Serializable]
    public class License
    {
        public List<Guid> LicenseIds { get; set; } = new List<Guid>();
        public string Owner { get; set; }

        public LicenseParameter RtuCount { get; set; }
        public LicenseParameter ClientStationCount { get; set; }
        public LicenseParameter WebClientCount { get; set; }
        public LicenseParameter SuperClientStationCount { get; set; }

        public string Version { get; set; } = @"2.0.0.0";
    }
}
