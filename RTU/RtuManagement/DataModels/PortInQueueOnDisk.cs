using System;
using Dto;
using Iit.Fibertest.DirectCharonLibrary;

namespace RtuManagement
{
    [Serializable]
    public class PortInQueueOnDisk
    {
        public NetAddress NetAddress { get; set; }

        public int OpticalPort { get; set; }

        public FiberState LastTraceState { get; set; }

        // for deserializer
        public PortInQueueOnDisk()
        {
            
        }

        public PortInQueueOnDisk(ExtendedPort port)
        {
            NetAddress = (NetAddress) port.NetAddress.Clone();
            OpticalPort = port.OpticalPort;
            LastTraceState = port.LastTraceState;
        }
    }

}