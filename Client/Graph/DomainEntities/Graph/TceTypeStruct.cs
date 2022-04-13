using System;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    [Serializable]
    public struct TceTypeStruct
    {
        public int Id{ get; set; }
        public bool IsVisible{ get; set; } // show only models user has
        public string Title{ get; set; }
        public TceMaker Maker{ get; set; }
        public string SoftwareVersion{ get; set; }
        public string Code{ get; set; } // for pretty parser switch
        public int SlotCount{ get; set; }
        public int[] Slots{ get; set; }
        public string Comment{ get; set; }
    }
}
