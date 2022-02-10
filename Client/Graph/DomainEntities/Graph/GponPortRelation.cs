using System;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    [Serializable]
    public class GponPortRelation
    {
        public Guid Id { get; set; }
        public Guid TceId { get; set; }
        public int GponInterface { get; set; }
        public Guid RtuId { get; set; }
        public OtauPortDto OtauPort { get; set; }
    }
}