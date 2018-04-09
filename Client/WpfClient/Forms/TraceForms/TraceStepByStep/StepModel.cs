using System;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class StepModel
    {
        public Guid NodeId { get; set; }
        public string Title { get; set; }
        public Guid EquipmentId { get; set; }

        public override string ToString()
        {
            return string.IsNullOrEmpty(Title) ? Resources.SID____noname_node_ : Title;
        }
    }
}