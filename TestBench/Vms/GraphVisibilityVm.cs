using System.Windows;

namespace Iit.Fibertest.TestBench
{
    public partial class GraphVm
    {
        private Visibility _equipmentVisibility;
        public Visibility EquipmentVisibility
        {
            get { return _equipmentVisibility; }
            set
            {
                if (value == _equipmentVisibility) return;
                _equipmentVisibility = value;
                NotifyOfPropertyChange();
            }
        }

        private bool _isEquipmentVisible;
        public bool IsEquipmentVisible
        {
            get { return _isEquipmentVisible; }
            set
            {
                if (value == _isEquipmentVisible) return;
                _isEquipmentVisible = value;
                NotifyOfPropertyChange();

                EquipmentVisibility = _isEquipmentVisible ? Visibility.Visible : Visibility.Hidden;
            }
        }
    }

}
