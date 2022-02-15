using System;
using System.Collections.Generic;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    public class AddOrUpdateTce
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public TceType TceType { get; set; }
        public string Ip { get; set; }
        public int SlotCount { get; set; }
        public List<TceSlot> Slots { get; set; } = new List<TceSlot>();
        public string Comment { get; set; }

    }
}
