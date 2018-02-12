using System;
using Caliburn.Micro;

namespace Iit.Fibertest.Client
{
    public class EquipmentOfChoiceModel : PropertyChangedBase
    {
        public Guid EquipmentId;

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (value == _isSelected) return;
                _isSelected = value;
                NotifyOfPropertyChange();
            }
        }

        public string TitleOfEquipment { get; set; }
        public string TypeOfEquipment { get; set; }

        public int LeftCableReserve { get; set; }
        public int RightCableReserve { get; set; }
        public bool IsRadioButtonEnabled { get; set; }
    }
}
