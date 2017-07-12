using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class FiberWithNodesAddViewModel : Screen
    {
        public bool Result { get; set; }
        public int Count { get; set; }

        public RadioButtonModel Node { get; set; } = new RadioButtonModel { Title = Resources.SID_Node, IsChecked = false };
        public RadioButtonModel AdjustmentNode { get; } = new RadioButtonModel { Title = Resources.SID_Adjustment_node, IsChecked = false };
        public RadioButtonModel CableReserve { get; set; } = new RadioButtonModel { Title = Resources.SID_CableReserve, IsChecked = false };
        public RadioButtonModel Sleeve { get; } = new RadioButtonModel { Title = Resources.SID_Closure, IsChecked = true };
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
            if (Node.IsChecked)
                return EquipmentType.Well;
            if (AdjustmentNode.IsChecked)
                return EquipmentType.Invisible;

            if (CableReserve.IsChecked)
                return EquipmentType.CableReserve;
            if (Sleeve.IsChecked)
                return EquipmentType.Closure;
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
                    Node.IsChecked = true;
                    break;
                case EquipmentType.Invisible:
                    AdjustmentNode.IsChecked = true;
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
                case EquipmentType.Closure:
                    Sleeve.IsChecked = true;
                    break;
            }
        }
    }
}
