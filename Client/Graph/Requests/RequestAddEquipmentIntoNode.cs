﻿using System;

namespace Iit.Fibertest.Graph
{
    public class RequestAddEquipmentIntoNode
    {
        public Guid NodeId { get; set; }
        public bool IsCableReserveRequested { get; set; }
    }
}