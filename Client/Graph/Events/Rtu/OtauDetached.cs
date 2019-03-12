using System;
using System.Collections.Generic;

namespace Iit.Fibertest.Graph
{
    [Serializable]
    public class OtauDetached
    {
        public Guid Id { get; set; }
        public Guid RtuId { get; set; }
        public string OtauIp { get; set; }
        public int TcpPort { get; set; }
        public List<Guid> TracesOnOtau { get; set; }
    }
}