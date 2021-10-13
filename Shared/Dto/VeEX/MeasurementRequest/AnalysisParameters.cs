using System.Collections.Generic;

// ReSharper disable InconsistentNaming
namespace Iit.Fibertest.Dto
{
    public class AnalysisParameters
    {
        public double macrobendThreshold { get; set; }
        public List<LasersParameter> lasersParameters { get; set; } = new List<LasersParameter>();
    }
}