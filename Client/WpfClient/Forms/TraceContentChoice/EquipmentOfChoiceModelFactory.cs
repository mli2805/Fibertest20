using System;
using System.Windows;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class EquipmentOfChoiceModelFactory
    {
        public EquipmentOfChoiceModel Create(Guid equipmentId, bool isLastNode)
        {
            var doNotUseOptionModel = new EquipmentOfChoiceModel()
            {
                EquipmentId = equipmentId,
                TypeOfEquipment = Resources.SID_Do_not_use,
                NameOfEquipment = "",
                IsTitleVisible = Visibility.Hidden,
                IsRadioButtonEnabled = !isLastNode,
            };
            return doNotUseOptionModel;
        }
       

        public EquipmentOfChoiceModel Create(Equipment equipment)
        {
            var equipmentOfChoiceModel = new EquipmentOfChoiceModel()
            {
                EquipmentId = equipment.Id,
                TypeOfEquipment = equipment.Type.ToLocalizedString(),
                NameOfEquipment = equipment.Title,
                IsRadioButtonEnabled = true,
            };
            return equipmentOfChoiceModel;
        }
    }
}