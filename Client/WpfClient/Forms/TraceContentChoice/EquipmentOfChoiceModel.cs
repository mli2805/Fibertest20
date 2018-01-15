using System;
using System.Windows;
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

        public string TypeOfEquipment { get; set; }
        public string NameOfEquipment { get; set; }
        public Visibility IsTitleVisible { get; set; } = Visibility.Visible;
        public bool IsRadioButtonEnabled { get; set; }
    }
}
