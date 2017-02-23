﻿using System;

namespace Iit.Fibertest.Graph
{
    /// <summary>
    /// Attention! Mind the difference with Fibertest 1.5
    /// This command for add node (well) only!
    /// Equipment should be added by separate command!
    /// </summary>
    public class AddNodeIntoFiber
    {
        public Guid Id { get; set; }
        public GpsLocation Position { get; set; }

        public Guid FiberId { get; set; }
        public Guid NewFiberId1 { get; set; }
        public Guid NewFiberId2 { get; set; }

    }
}
