using System;
using System.Collections.Generic;

namespace Iit.Fibertest.Dto
{
    [Serializable]
    public class BranchOfAcceptableMeasParams
    {
        public Dictionary<string, LeafOfAcceptableMeasParams> Distances { get; set; } = new Dictionary<string, LeafOfAcceptableMeasParams>();
        public double BackscatteredCoefficient { get; set; }
        public double RefractiveIndex { get; set; }
    }
}