﻿using System;
using System.Collections.Generic;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    public class Trace
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public Guid RtuId { get; set; } // лучше хранить, т.к. поиск через список РТУ...
        public OtauPortDto OtauPort { get; set; } // лучше сохранять при атаче к порту, т.к. очень сложный поиск
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

        public bool HasBase => PreciseId != Guid.Empty || FastId != Guid.Empty || AdditionalId != Guid.Empty;
        public bool ReadyForMonitoring => PreciseId != Guid.Empty && FastId != Guid.Empty;
        public bool IsIncludedInMonitoringCycle { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }
}
