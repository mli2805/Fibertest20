using System;

namespace Iit.Fibertest.Client
{
    public class RequestAddNodeIntoFiber
    {
        public Guid FiberId { get; set; }

        public bool IsAdjustmentNode { get; set; }
    }
}
