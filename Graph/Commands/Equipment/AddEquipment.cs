using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iit.Fibertest.Graph.Commands
{
    public class AddEquipment
    {
        public Guid Id { get; set; }
        public Guid NodeId { get; set; }
        public string Title { get; set; }
        public EquipmentType Type { get; set; }
        public int CableReserveLeft { get; set; }
        public int CableReserveRight { get; set; }
        public string Comment { get; set; }
    }
}
