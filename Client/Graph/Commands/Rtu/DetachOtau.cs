using System;
using System.Collections.Generic;

namespace Iit.Fibertest.Graph
{
    public class DetachOtau
    {
        public Guid Id { get; set; }
        public Guid RtuId { get; set; }
        public List<Guid> TracesOnOtau { get; set; }

    }
}