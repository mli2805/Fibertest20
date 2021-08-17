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

        /// <summary>
        /// set relation between gpon interface of olt and rtu port
        /// </summary>
        /// dictionary key is number of gpon interface of olt
        /// tuple's item1 is rtu guid, item2 is rtu port (mind bops)
        // public Dictionary<int, Tuple<Guid, OtauPortDto>> Relations { get; set; }

        public List<GponPortRelation> Relations { get; set; } = new List<GponPortRelation>();
    }
}