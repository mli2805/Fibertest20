using System.Collections.Generic;

// ReSharper disable InconsistentNaming
namespace Iit.Fibertest.Dto
{
    public class VeexMeasurementRequest
    {
        public string id { get; set; }
        public string otdrId { get; set; }
        public VeexMeasOtdrParameters otdrParameters { get; set; }
        public GeneralParameters generalParameters { get; set; } = new GeneralParameters();
        public AnalysisParameters analysisParameters { get; set; } = new AnalysisParameters();
        public SpanParameters spanParameters { get; set; } = new SpanParameters();
        public bool suspendMonitoring { get; set; }
        public List<VeexOtauPort> otauPorts { get; set; }
    }
}