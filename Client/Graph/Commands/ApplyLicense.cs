using System;

namespace Iit.Fibertest.Graph
{
    public class ApplyLicense
    {
        public Guid LicenseId { get; set; }
        public bool IsIncremental { get; set; } // by default = false -> Main license
        public string Owner { get; set; }

        public LicenseParameter RtuCount { get; set; } = new LicenseParameter();
        public LicenseParameter ClientStationCount { get; set; } = new LicenseParameter();
        public LicenseParameter WebClientCount { get; set; } = new LicenseParameter();
        public LicenseParameter SuperClientStationCount { get; set; } = new LicenseParameter();

        public DateTime CreationDate { get; set; } // Used in LicenseKey string
        public DateTime LoadingDate { get; set; } // for evaluations
        public string Version { get; set; } = @"2.0.0.0";
    }
}