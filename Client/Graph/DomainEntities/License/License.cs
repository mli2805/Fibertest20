using System;

namespace Iit.Fibertest.Graph
{
    [Serializable]
    public class License
    {
        public Guid LicenseId { get; set; }
        public string LicenseKey => Lk();

        private string Lk()
        {
            var id = LicenseId.ToString().ToUpper().Substring(0, 8);
            var licType = IsIncremental ? @"I" : IsMachineKeyRequired ? @"BR" : @"BF";
            var stations = $@"{ClientStationCount.Value:D2}{WebClientCount.Value:D2}{SuperClientStationCount.Value:D2}";
            return $@"FT020-{id}-{licType}{RtuCount.Value:D2}{stations}-{CreationDate:yyMMdd}";
        }
        
        public bool IsIncremental { get; set; } // by default = false -> Main license
        public string Owner { get; set; }

        public LicenseParameter RtuCount { get; set; }
        public LicenseParameter ClientStationCount { get; set; }
        public LicenseParameter WebClientCount { get; set; }
        public LicenseParameter SuperClientStationCount { get; set; }

        public bool IsMachineKeyRequired { get; set; }
        public string SecurityAdminPassword { get; set; }

        public DateTime CreationDate { get; set; } // Used in LicenseKey string
        public DateTime LoadingDate { get; set; } // for evaluations
        public string Version { get; set; } = @"2.0.0.0";

        public override string ToString()
        {
            return Lk();
        }
    }
}
