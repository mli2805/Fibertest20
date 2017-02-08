using System;
using System.Collections.Generic;

namespace TreeVision.Vms
{
    public class TraceVm
    {
        public Guid Id { get; set; }

        public List<Guid> Nodes { get; set; }
        public List<Guid> Equipments { get; set; }
        public Guid PreciseId { get; set; } = Guid.Empty;
        public Guid FastId { get; set; } = Guid.Empty;
        public Guid AdditionalId { get; set; } = Guid.Empty;
        public string Comment { get; set; }

        public bool HasBase => PreciseId != Guid.Empty || FastId != Guid.Empty || AdditionalId != Guid.Empty;
    }
}