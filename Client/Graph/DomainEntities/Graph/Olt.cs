using System;
using System.Collections.Generic;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    [Serializable]
    public class Olt
    {
        public Guid Id { get; set; }
        public string Ip { get; set; }
        public OltModel OltModel { get; set; }
        public List<GponPortRelation> Relations { get; set; } = new List<GponPortRelation>();
    }
}