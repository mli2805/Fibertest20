using System;

namespace Iit.Fibertest.Dto
{
    public class TraceDto
    {
        public Guid TraceId;
        public Guid RtuId;
        public string Title;
        public OtauPortDto OtauPort;
        public bool IsAttached;
        public int Port;

        public FiberState State;

        public bool HasEnoughBaseRefsToPerformMonitoring;
        public bool IsIncludedInMonitoringCycle;
    }
}