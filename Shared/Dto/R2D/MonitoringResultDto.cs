using System;
using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [Flags]
    public enum ReasonToSendMonitoringResult
    {
        None = 0,
        FirstMeasurementOnPort = 1,
        TraceStateChanged = 2,
        OpticalAccidentConfirmation = 4,
        MonitoringModeChanged = 8,
        TimeToRegularSave = 16,
        OutOfTurnPreciseMeasurement = 32,
        MeasurementAccidentStatusChanged = 64, // base not found, invalid base
    }

    [Serializable]
    [DataContract]
    public class MonitoringResultDto
    {
        [DataMember]
        public ReturnCode ReturnCode { get; set; }

        [DataMember]
        public ReasonToSendMonitoringResult Reason { get; set; }

        [DataMember]
        public Guid RtuId { get; set; }

        [DataMember]
        public DateTime TimeStamp { get; set; }

        [DataMember]
        public PortWithTraceDto PortWithTrace { get; set; }

        [DataMember]
        public BaseRefType BaseRefType { get; set; }

        [DataMember]
        public FiberState TraceState { get; set; }

        [DataMember]
        public byte[] SorBytes { get; set; }
    }
}