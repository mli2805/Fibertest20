using Iit.Fibertest.Dto;

namespace Iit.Fibertest.RtuMngr
{
    public class MonitoringResultEf
    {
        public int Id { get; set; }

        public ReturnCode ReturnCode { get; set; }

        public ReasonToSendMonitoringResult Reason { get; set; }

        public Guid RtuId { get; set; }

        public DateTime TimeStamp { get; set; }

        #region PortWithTrace
        //public PortWithTraceDto PortWithTrace { get; set; } = null!;
        public string Serial { get; set; } = null!;
        public bool IsPortOnMainCharon { get; set; }
        public int OpticalPort { get; set; }
        public Guid TraceId { get; set; }
        #endregion
       

        public BaseRefType BaseRefType { get; set; }

        public FiberState TraceState { get; set; }

        public byte[] SorBytes { get; set; } = null!;
    }
}
