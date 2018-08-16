using System;

namespace Iit.Fibertest.Graph
{
    public class ApplyLicense
    {
        public Guid LicenseId { get; set; }
        public string Owner { get; set; }

        public LicenseParameter RtuCount { get; set; } = new LicenseParameter();
        public LicenseParameter ClientStationCount { get; set; } = new LicenseParameter();
        public LicenseParameter SuperClientStationCount { get; set; } = new LicenseParameter();

        public string Version { get; set; } = @"2.0.0.0";
    }
}