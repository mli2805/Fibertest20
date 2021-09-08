using System.Collections.Generic;

// ReSharper disable InconsistentNaming
namespace Iit.Fibertest.Dto
{
    public class VeexMeasurementRequest
    {
        public string id { get; set; }
        public string otdrId { get; set; }
        public VeexMeasOtdrParameters otdrParameters { get; set; }
        public GeneralParameters generalParameters { get; set; }
        public AnalysisParameters analysisParameters { get; set; }
        public SpanParameters spanParameters { get; set; }
        public bool suspendMonitoring { get; set; }
        public List<VeexOtauPort> otauPorts { get; set; }
    }
}