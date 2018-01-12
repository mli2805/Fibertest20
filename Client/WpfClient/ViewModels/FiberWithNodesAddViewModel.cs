using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class FiberWithNodesAddViewModel : Screen
    {
        public bool Result { get; set; }
        public int Count { get; set; }

        public RadioButtonModel AdjustmentPoint { get; } = new RadioButtonModel { Title = Resources.SID_Adjustment_point, IsChecked = false };
        public RadioButtonModel NodeWithoutEquipment { get; set; } = new RadioButtonModel { Title = Resources.SID_Node_without_equipment, IsChecked = false };
        public RadioButtonModel CableReserve { get; set; } = new RadioButtonModel { Title = Resources.SID_CableReserve, IsChecked = false };
        public RadioButtonModel Closure { get; } = new RadioButtonModel { Title = Resources.SID_Closure, IsChecked = true };
        public RadioButtonModel Cross { get; } = new RadioButtonModel { Title = Resources.SID_Cross, IsChecked = false };
        public RadioButtonModel Other { get; } = new RadioButtonModel { Title = Resources.SID_Other, IsChecked = false };
        public RadioButtonModel Terminal { get; } = new RadioButtonModel { Title = Resources.SID_Terminal, IsChecked = false };

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
            if (AdjustmentPoint.IsChecked)
                return EquipmentType.AdjustmentPoint;
            if (NodeWithoutEquipment.IsChecked)
                return EquipmentType.EmptyNode;
            if (CableReserve.IsChecked)
                return EquipmentType.CableReserve;

            if (Closure.IsChecked)
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
                case EquipmentType.AdjustmentPoint:
                    AdjustmentPoint.IsChecked = true;
                    break;
                case EquipmentType.EmptyNode:
                    NodeWithoutEquipment.IsChecked = true;
                    break;
                case EquipmentType.CableReserve:
                    CableReserve.IsChecked = true;
                    break;

                case EquipmentType.Closure:
                    Closure.IsChecked = true;
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
            }
        }
    }
}
