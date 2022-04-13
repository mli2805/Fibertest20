using System;
using System.Collections.Generic;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    [Serializable]
    public class Tce
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public TceType TceType { get; set; }
        public string Ip { get; set; }
        public int SlotCount { get; set; }
        public List<TceSlot> Slots { get; set; } = new List<TceSlot>();
        public string Comment { get; set; }
    }

    [Serializable]
    public class TceSlot
    {
        public int Position { get; set; }
        public bool IsPresent { get; set; }
        public int GponInterfaceCount { get; set; }
    }

    [Serializable]
    public class TceS
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; }
        public TceTypeStruct TceTypeStruct { get; set; }
        public string Ip { get; set; }
        public int SlotCount { get; set; }
        public List<TceSlot> Slots { get; set; } = new List<TceSlot>();
        public string Comment { get; set; }
    }
}