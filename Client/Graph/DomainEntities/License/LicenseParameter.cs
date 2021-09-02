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
            Value = licenseParameterInFile?.Value ?? -1;
            ValidUntil = licenseParameterInFile != null 
                ? licenseParameterInFile.IsTermInYears
                    ? DateTime.Today.AddYears(licenseParameterInFile.Term)
                    : DateTime.Today.AddMonths(licenseParameterInFile.Term) 
                : DateTime.Today.AddDays(-1);
        }

        public override string ToString()
        {
            return $@"{Value}   ({Resources.SID_valid_until} {ValidUntil:dd MMMM yyyy}) ";
        }
    }
}