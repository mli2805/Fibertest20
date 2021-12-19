using System.Collections.Generic;

namespace Iit.Fibertest.Client
{
    public class RenderingResult
    {
        public readonly List<NodeVm> NodeVms = new List<NodeVm>();
        public readonly List<FiberVm> FiberVms = new List<FiberVm>();
    }
}