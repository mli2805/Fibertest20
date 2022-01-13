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

        public bool IsMachineKeyRequired { get; set; }
        public byte[] SecurityAdminPassword { get; set; }
        public DateTime CreationDate { get; set; } // Used in LicenseKey string
        public DateTime LoadingDate { get; set; } // for evaluations
        public string Version { get; set; } = @"2.0.0.0";

        public string Lk()
        {
            var id = LicenseId.ToString().ToUpper().Substring(0, 8);
            var licType = IsIncremental ? @"I" : IsMachineKeyRequired ? @"BR" : @"BF";
            var stations = $@"{ClientStationCount.Value:D2}{WebClientCount.Value:D2}{SuperClientStationCount.Value:D2}";
            return $@"FT020-{id}-{licType}{RtuCount.Value:D2}{stations}-{CreationDate:yyMMdd}";
        }
    }
}