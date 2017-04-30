using System;
using System.Collections.Generic;
using Iit.Fibertest.Utils35;

namespace Iit.Fibertest.Client
{
    public class TraceVm
    {
        public Guid Id { get; set; }
        public Guid RtuId { get; set; }

        public FiberState State { get; set; } = FiberState.NotJoined;
        public int Port { get; set; }
        public List<Guid> Nodes { get; set; }

        public Guid PreciseId { get; set; } = Guid.Empty;
        public Guid FastId { get; set; } = Guid.Empty;
        public Guid AdditionalId { get; set; } = Guid.Empty;
        public string Title { get; set; }

        public bool HasBase => PreciseId != Guid.Empty || FastId != Guid.Empty || AdditionalId != Guid.Empty;

        public override string ToString()
        {
            return Title;
        }
    }
}