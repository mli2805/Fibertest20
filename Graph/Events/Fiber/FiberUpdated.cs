﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iit.Fibertest.Graph.Events
{
    public class FiberUpdated
    {
        public Guid Id { get; set; }
        public double UserInputedLength { get; set; }
    }
}
