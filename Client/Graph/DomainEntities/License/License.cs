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
            var licType = IsReplacementLicense ? "I" : "B";
            return $@"FT020-{id}-{licType}{RtuCount.Value:D2}{ClientStationCount.Value:D2}{WebClientCount.Value:D2}{SuperClientStationCount.Value:D2}-{CreationDate:yyMMdd}";
        }
        
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
