using System.Collections.Generic;

namespace Iit.Fibertest.Dto
{
    public class BranchOfAcceptableMeasParams
    {
        public Dictionary<string, LeafOfAcceptableMeasParams> Distances { get; set; } = new Dictionary<string, LeafOfAcceptableMeasParams>();
    }
}