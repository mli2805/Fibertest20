﻿using System;

namespace Iit.Fibertest.Graph.Events
{
    public class NodeIntoFiberAdded
    {
        public Guid Id { get; set; }
        public Guid FiberId { get; set; }
    }
}
