using System;
using Dto;

namespace RtuManagement
{
    [Serializable]
    public class MonitoringPortOnDisk
    {
        public NetAddress NetAddress { get; set; }
        public int OpticalPort { get; set; }
        public FiberState LastTraceState { get; set; }

        public DateTime LastPreciseSavedTimestamp { get; set; }
        public DateTime LastFastSavedTimestamp { get; set; }

        public bool MonitoringModeChangedFlag { get; set; }
       
        // for deserializer
        public MonitoringPortOnDisk()
        {
            
        }

        public MonitoringPortOnDisk(MonitorigPort port)
        {
            NetAddress = (NetAddress) port.NetAddress.Clone();
            OpticalPort = port.OpticalPort;
            LastTraceState = port.LastTraceState;

            LastFastSavedTimestamp = port.LastFastSavedTimestamp;
            LastPreciseSavedTimestamp = port.LastPreciseSavedTimestamp;

            MonitoringModeChangedFlag = port.MonitoringModeChangedFlag;
        }
    }

}