using System;
using System.Collections.Generic;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    public class ChangeMonitoringSettings
    {
        public Guid RtuId { get; set; }

        public List<Guid> TracesInMonitoringCycle { get; set; }

        public Frequency PreciseMeas { get; set; }
        public Frequency PreciseSave { get; set; }
        public Frequency FastSave { get; set; }

        public bool IsMonitoringOn { get; set; }
    }
}