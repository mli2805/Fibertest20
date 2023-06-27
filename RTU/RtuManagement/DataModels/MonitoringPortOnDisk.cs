using System;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.RtuManagement
{
    [Serializable]
    public class MonitoringPortOnDisk
    {
        public string Serial { get; set; }
        public int OpticalPort { get; set; }
        public bool IsPortOnMainCharon { get; set; }
        public Guid TraceId { get; set; }
        public FiberState LastTraceState { get; set; }
        public MoniResult LastMoniResult { get; set; }

        public DateTime? LastPreciseMadeTimestamp { get; set; }
        public DateTime LastPreciseSavedTimestamp { get; set; }
        public DateTime LastFastSavedTimestamp { get; set; }

        public bool IsMonitoringModeChanged { get; set; }
        public bool IsConfirmationRequired { get; set; }
       
        // for deserializer
        public MonitoringPortOnDisk()
        {
            
        }

        public MonitoringPortOnDisk(MonitoringPort port)
        {
            Serial = port.CharonSerial;
            OpticalPort = port.OpticalPort;
            IsPortOnMainCharon = port.IsPortOnMainCharon;
            TraceId = port.TraceId;
            LastTraceState = port.LastTraceState;

            if (port.LastMoniResult != null)
                LastMoniResult = new MoniResult()
                {
                    ReturnCode = port.LastMoniResult.ReturnCode,
                    IsNoFiber = port.LastMoniResult.IsNoFiber,
                    IsFiberBreak = port.LastMoniResult.IsFiberBreak,
                    Levels = port.LastMoniResult.Levels,
                    BaseRefType = port.LastMoniResult.BaseRefType,
                    FirstBreakDistance = port.LastMoniResult.FirstBreakDistance,
                    Accidents = port.LastMoniResult.Accidents,
                };

            LastPreciseMadeTimestamp = port.LastPreciseMadeTimestamp;
            LastFastSavedTimestamp = port.LastFastSavedTimestamp;
            LastPreciseSavedTimestamp = port.LastPreciseSavedTimestamp;

            IsMonitoringModeChanged = port.IsMonitoringModeChanged;
            IsConfirmationRequired = port.IsConfirmationRequired;
        }
    }

}