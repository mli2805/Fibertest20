using System;

namespace Iit.Fibertest.Graph
{
    [Serializable]
    public class LicenseParameterInFile
    {
        public int Value { get; set; } = 1;

        public int Term { get; set; } = 3;
        public bool IsTermInYears { get; set; } // Or in months
    }
}