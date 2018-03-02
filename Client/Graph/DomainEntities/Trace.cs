using System;
using System.Collections.Generic;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    public class Trace
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public Guid RtuId { get; set; } // it's better to store than search through the RTU list

        public FiberState State { get; set; } = FiberState.NotJoined;
        public OtauPortDto OtauPort { get; set; } // it's better to store due attach to port, cos search is too complicated
        public bool IsAttached => OtauPort != null;
        public int Port { get; set; } = -1;

        public TraceMode Mode { get; set; } = TraceMode.Light;
        public List<Guid> Nodes { get; set; } = new List<Guid>();
        public List<Guid> Equipments { get; set; } = new List<Guid>();

        public Guid PreciseId { get; set; } = Guid.Empty;
        public TimeSpan PreciseDuration { get; set; }
        public Guid FastId { get; set; } = Guid.Empty;
        public TimeSpan FastDuration { get; set; }
        public Guid AdditionalId { get; set; } = Guid.Empty;
        public TimeSpan AdditionalDuration { get; set; }
        public string Comment { get; set; }

        public bool HasAnyBaseRef => PreciseId != Guid.Empty || FastId != Guid.Empty || AdditionalId != Guid.Empty;
        public bool HasEnoughBaseRefsToPerformMonitoring => PreciseId != Guid.Empty && FastId != Guid.Empty;
        public bool IsIncludedInMonitoringCycle { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }
}
