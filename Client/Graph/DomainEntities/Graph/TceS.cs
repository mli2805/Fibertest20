using System;
using System.Collections.Generic;

namespace Iit.Fibertest.Graph
{
    [Serializable]
    public class TceS
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; }
        public TceTypeStruct TceTypeStruct { get; set; }
        public string Ip { get; set; } = @"0.0.0.0";
        public List<TceSlot> Slots { get; set; } = new List<TceSlot>();
        public string Comment { get; set; }
    }
}