using System;
using System.Collections.Generic;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Client
{
    public class TraceVm
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public Guid RtuId { get; set; }

        public FiberState State { get; set; } = FiberState.NotJoined;
        public int Port { get; set; }
        public List<Guid> Nodes { get; set; }
        public List<Guid> Equipments { get; set; } = new List<Guid>();

        public override string ToString()
        {
            return Title;
        }
    }
}