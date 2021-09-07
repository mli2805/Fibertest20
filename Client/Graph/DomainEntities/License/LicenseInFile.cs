using System;

namespace Iit.Fibertest.Graph
{
    [Serializable]
    public class LicenseInFile
    {
        public Guid LicenseId { get; set; } = Guid.NewGuid();
        public bool IsIncremental { get; set; } // by default = false -> Main license
        public string Owner { get; set; }

        public LicenseParameterInFile RtuCount { get; set; } = new LicenseParameterInFile();
        public LicenseParameterInFile ClientStationCount { get; set; } = new LicenseParameterInFile();
        public LicenseParameterInFile WebClientCount { get; set; } = new LicenseParameterInFile();
        public LicenseParameterInFile SuperClientStationCount { get; set; } = new LicenseParameterInFile();

        public DateTime CreationDate { get; set; } // Used in LicenseKey string
        public DateTime LoadingDate { get; set; } // for evaluations
        public string Version { get; set; } = @"2.0.0.0";
    }
}