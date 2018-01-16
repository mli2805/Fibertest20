using System.Collections.Generic;

namespace Iit.Fibertest.Dto
{
    public class TreeOfAcceptableMeasParams
    {
        public Dictionary<string, BranchOfAcceptableMeasParams> Units { get; set; } = new Dictionary<string, BranchOfAcceptableMeasParams>();
    }
}
