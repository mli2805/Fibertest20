using Caliburn.Micro;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.TestBench
{
    public class AddFiberWithNodesViewModel : Screen
    {
        public string CountLabel { get; set; } = "Количество";
        public string Type { get; set; } = "Тип";

        public bool Result { get; set; }
        public int Count { get; set; }


        public RadioButtonModel Well { get; set; } = new RadioButtonModel { Title = "Well", IsChecked = false };
        public RadioButtonModel Invisible { get; } = new RadioButtonModel { Title = "Invisible", IsChecked = false };
        public RadioButtonModel CableReserve { get; set; } = new RadioButtonModel { Title = "CableReserve", IsChecked = false };
        public RadioButtonModel Sleeve { get; } = new RadioButtonModel { Title = "Sleeve", IsChecked = true };
        public RadioButtonModel Cross { get; } = new RadioButtonModel { Title = "Cross", IsChecked = false };
        public RadioButtonModel Terminal { get; } = new RadioButtonModel { Title = "Terminal", IsChecked = false };
        public RadioButtonModel Other { get; } = new RadioButtonModel { Title = "Other", IsChecked = false };

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Участок с узлами";
        }

        public void Ok()
        {
            Result = true;
            TryClose();
        }

        public void Cancel()
        {
            Result = false;
            TryClose();
        }

        public EquipmentType GetSelectedType()
        {
            if (Well.IsChecked)
                return EquipmentType.Well;
            if (Invisible.IsChecked)
                return EquipmentType.Invisible;

            if (CableReserve.IsChecked)
                return EquipmentType.CableReserve;
            if (Sleeve.IsChecked)
                return EquipmentType.Sleeve;
            if (Cross.IsChecked)
                return EquipmentType.Cross;
            if (Terminal.IsChecked)
                return EquipmentType.Terminal;
            //else if (Other.IsSelected)
            return EquipmentType.Other;
        }
    }
}
