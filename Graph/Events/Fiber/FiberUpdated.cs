﻿using System;

namespace Iit.Fibertest.Graph.Events
{
    public class FiberUpdated
    {
        public Guid Id { get; set; }
        public double UserInputedLength { get; set; }
    }
}
