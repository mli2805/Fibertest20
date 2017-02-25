using System;
using System.Collections.Generic;

namespace Iit.Fibertest.Graph
{
    public class Trace
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public Guid RtuId { get; set; } // лучше хранить, т.к. поиск через список РТУ...
        public int Port { get; set; } = -1;

        public List<Guid> Nodes { get; set; } = new List<Guid>();
        public List<Guid> Equipments { get; set; } = new List<Guid>();

        public Guid PreciseId { get; set; } = Guid.Empty;
        public Guid FastId { get; set; } = Guid.Empty;
        public Guid AdditionalId { get; set; } = Guid.Empty;
        public string Comment { get; set; }

        public bool HasBase => PreciseId != Guid.Empty || FastId != Guid.Empty || AdditionalId != Guid.Empty;

        public override string ToString()
        {
            return Title;
        }
    }
}
