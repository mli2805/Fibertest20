using System;
using System.Collections.Generic;

namespace Iit.Fibertest.TestBench
{
    public class TraceVm
    {
        public Guid Id { get; set; }
        public Guid RtuId { get; set; }

        public int Port { get; set; }
        public List<Guid> Nodes { get; set; }
        public List<Guid> Equipments { get; set; }

        public Guid PreciseId { get; set; } = Guid.Empty;
        public Guid FastId { get; set; } = Guid.Empty;
        public Guid AdditionalId { get; set; } = Guid.Empty;
        public string Title { get; set; }
        public string Comment { get; set; }

        public bool HasBase => PreciseId != Guid.Empty || FastId != Guid.Empty || AdditionalId != Guid.Empty;

        public override string ToString()
        {
            return Title;
        }
    }
}