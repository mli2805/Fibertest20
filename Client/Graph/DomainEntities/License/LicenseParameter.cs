using System;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Graph
{
    [Serializable]
    public class LicenseParameter
    {
        public int Value { get; set; } = -1;

        public DateTime ValidUntil { get; set; }

        public LicenseParameter(){}

        public LicenseParameter(LicenseParameterInFile licenseParameterInFile)
        {
            Value = licenseParameterInFile.Value;
            ValidUntil = licenseParameterInFile.IsTermInYears
                ? DateTime.Today.AddYears(licenseParameterInFile.Term)
                : DateTime.Today.AddMonths(licenseParameterInFile.Term);
        }

        public override string ToString()
        {
            var text = Resources.SID_valid_until;
            return $@"{Value}   ({text} {ValidUntil:d}) ";
        }
    }
}