using System;

namespace Iit.Fibertest.Graph
{
    public class ApplyLicense
    {
        public Guid LicenseId { get; set; }
        public string Owner { get; set; }

        public LicenseParameter RtuCount { get; set; }
        public LicenseParameter ClientStationCount { get; set; }
        public LicenseParameter SuperClientStationCount { get; set; }

        public string Version { get; set; } = @"2.0.0.0";
    }
}