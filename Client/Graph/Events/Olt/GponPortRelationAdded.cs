using System;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    public class GponPortRelationAdded
    {
        public Guid Id { get; set; }
        public Guid TceId { get; set; }
        public int TceSlot { get; set; }
        public int GponInterface { get; set; }
        public Guid RtuId { get; set; }
        public OtauPortDto OtauPort { get; set; }
    }
}
