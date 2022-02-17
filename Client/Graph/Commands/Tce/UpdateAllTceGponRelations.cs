using System;
using System.Collections.Generic;

namespace Iit.Fibertest.Graph
{
    public class UpdateAllTceGponRelations
    {
        public Guid TceId { get; set; }
        public List<GponPortRelation> AllTceRelations { get; set; } = new List<GponPortRelation>();
    }
}
