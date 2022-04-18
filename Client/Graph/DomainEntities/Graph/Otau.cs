using System;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Graph
{
    [Serializable]
    public class Otau
    {
        public Guid Id { get; set; }
        public Guid RtuId { get; set; }
        public string VeexRtuMainOtauId { get; set; }

        public NetAddress NetAddress { get; set; } = new NetAddress();
        public string Serial { get; set; }
        public int PortCount { get; set; }

        public int MasterPort { get; set; }
        public bool IsOk { get; set; }

        public string Title =>
            VeexRtuMainOtauId != null && VeexRtuMainOtauId.StartsWith(@"S1") 
                ? Resources.SID_Main
                : Id == RtuId ? @"---" : NetAddress.Ip4Address;
    }
}