using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.TestBench
{
    public class FiberWithNodesAddViewModel : Screen
    {
        public string CountLabel { get; set; } = Resources.SID_Quantity;
        public string Type { get; set; } = Resources.SID_Type;

        public bool Result { get; set; }
        public int Count { get; set; }


        public RadioButtonModel Well { get; set; } = new RadioButtonModel { Title = Resources.SID_Well, IsChecked = false };
        public RadioButtonModel Invisible { get; } = new RadioButtonModel { Title = Resources.SID_Invisible, IsChecked = false };
        public RadioButtonModel CableReserve { get; set; } = new RadioButtonModel { Title = Resources.SID_CableReserve, IsChecked = false };
        public RadioButtonModel Sleeve { get; } = new RadioButtonModel { Title = Resources.SID_Sleeve, IsChecked = true };
        public RadioButtonModel Cross { get; } = new RadioButtonModel { Title = Resources.SID_Cross, IsChecked = false };
        public RadioButtonModel Terminal { get; } = new RadioButtonModel { Title = Resources.SID_Terminal, IsChecked = false };
        public RadioButtonModel Other { get; } = new RadioButtonModel { Title = Resources.SID_Other, IsChecked = false };

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Section_with_nodes;
        }

        public void Save()
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

        public void SetSelectedType(EquipmentType type)
        {
            switch (type)
            {
                case EquipmentType.Well:
                    Well.IsChecked = true;
                    break;
                case EquipmentType.Invisible:
                    Invisible.IsChecked = true;
                    break;
                case EquipmentType.CableReserve:
                    CableReserve.IsChecked = true;
                    break;
                case EquipmentType.Cross:
                    Cross.IsChecked = true;
                    break;
                case EquipmentType.Terminal:
                    Terminal.IsChecked = true;
                    break;
                case EquipmentType.Other:
                    Other.IsChecked = true;
                    break;
                case EquipmentType.Sleeve:
                    Sleeve.IsChecked = true;
                    break;

            }
        }
    }
}
