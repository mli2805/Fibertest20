using System;

namespace Iit.Fibertest.Dto
{
    public class RtuDto
    {
        public Guid RtuId { get; set; }
        public MonitoringState MonitoringMode { get; set; }
        public string Title { get; set; }

        public string Version { get; set; }
        public string Version2 { get; set; }

    }
}
