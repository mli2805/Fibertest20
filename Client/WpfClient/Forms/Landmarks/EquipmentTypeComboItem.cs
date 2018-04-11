using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class EquipmentTypeComboItem
    {
        public EquipmentType Type { get; set; }

        public EquipmentTypeComboItem(EquipmentType type)
        {
            Type = type;
        }

        public override string ToString()
        {
            return Type.ToLocalizedString();
        }
    }
}