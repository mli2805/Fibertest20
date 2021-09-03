using System;

namespace Iit.Fibertest.Graph
{
    public class LicenseApplied
    {
        public Guid LicenseId { get; set; }
        public bool IsReplacementLicense { get; set; } // by default = false -> Additional license
        public string Owner { get; set; }
       
        public LicenseParameter RtuCount { get; set; }
        public LicenseParameter ClientStationCount { get; set; }
        public LicenseParameter WebClientCount { get; set; }
        public LicenseParameter SuperClientStationCount { get; set; }

        public DateTime CreationDate { get; set; } // Used in LicenseKey string
        public DateTime LoadingDate { get; set; } // for evaluations
        public string Version { get; set; } = @"2.0.0.0";
    }
}