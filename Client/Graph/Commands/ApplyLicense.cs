using System;

namespace Iit.Fibertest.Graph
{
    public class ApplyLicense
    {
        public Guid LicenseId { get; set; }
        public Guid UserId { get; set; } // user who sends license to server
        public bool IsIncremental { get; set; } // by default = false -> Main license
        public string Owner { get; set; }

        public LicenseParameter RtuCount { get; set; } = new LicenseParameter();
        public LicenseParameter ClientStationCount { get; set; } = new LicenseParameter();
        public LicenseParameter WebClientCount { get; set; } = new LicenseParameter();
        public LicenseParameter SuperClientStationCount { get; set; } = new LicenseParameter();

        public bool IsMachineKeyRequired { get; set; }
        public string SecurityAdminPassword { get; set; }
        public Guid AdminUserId { get; set; }
        public string MachineKey { get; set; }

        public DateTime CreationDate { get; set; } // Used in LicenseKey string
        public DateTime LoadingDate { get; set; } // for evaluations
        public string Version { get; set; } = @"2.0.0.0";
    }
}