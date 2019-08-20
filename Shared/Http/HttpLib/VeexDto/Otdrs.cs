using System.Collections.Generic;

namespace HttpLib
{
    public class Otdrs
    {
        public List<EquipmentItem> items { get; set; }
        public int offset { get; set; }
        public int total { get; set; }
    }
}