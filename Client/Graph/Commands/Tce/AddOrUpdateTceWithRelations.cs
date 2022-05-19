using System;
using System.Collections.Generic;

namespace Iit.Fibertest.Graph
{
    public class AddOrUpdateTceWithRelations
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public TceTypeStruct TceTypeStruct { get; set; }
        public string Ip { get; set; }
        public List<TceSlot> Slots { get; set; } = new List<TceSlot>();
        public bool ProcessSnmpTraps { get; set; }
        public string Comment { get; set; }

        public List<GponPortRelation> AllRelationsOfTce { get; set; } = new List<GponPortRelation>();
    }
}