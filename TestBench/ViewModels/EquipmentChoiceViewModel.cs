using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.TestBench
{
    public class EquipmentChoiceViewModel : Screen
    {
        private readonly List<Equipment> _possibleEquipment;
        private readonly bool _isLastNode;
        public bool IsClosed { get; set; }

        public string Caption { get; set; }
        public List<RadioButtonModel> Choices { get; set; } // for binding

        public bool ShouldWeContinue { get; set; }
        public bool ShouldEquipmentViewBeOpen { get; set; }

        public EquipmentChoiceViewModel(List<Equipment> possibleEquipment, bool isLastNode)
        {
            _possibleEquipment = possibleEquipment;
            _isLastNode = isLastNode;
            InitializeChoices();
        }

        private void InitializeChoices()
        {
            Caption = "Выберите оборудование для трассы";
            Choices = new List<RadioButtonModel>();
            foreach (var equipment in _possibleEquipment)
            {
                var radioButtonModel = new RadioButtonModel {Title = equipment.Title, IsChecked = equipment == _possibleEquipment.First()};
                radioButtonModel.PropertyChanged += RadioButtonModel_PropertyChanged;
                Choices.Add(radioButtonModel);
            }
            Choices.Add(new RadioButtonModel {Title = "Не использовать", IsChecked = false, IsEnabled = !_isLastNode});
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Выбор оборудования";
        }

        private void RadioButtonModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RadioButtonModel model = (RadioButtonModel)sender;
            if (e.PropertyName == "IsChecked" && model.IsChecked)
            {
                foreach (var radioButtonModel in Choices.Where(m => m != model))
                {
                    radioButtonModel.IsChecked = false;
                }
            }
        }
        public Guid GetSelectedEquipmentGuid()
        {
            return GetCheckedRadioButton() == _possibleEquipment.Count ? Guid.Empty : _possibleEquipment[GetCheckedRadioButton()].Id;
        }

        private int GetCheckedRadioButton()
        {
            foreach (var myRadioButton in Choices)
            {
                if (myRadioButton.IsChecked)
                    return Choices.IndexOf(myRadioButton);
            }
            return -1; 
        }

        public void UseButton()
        {
            ShouldWeContinue = true;
            ShouldEquipmentViewBeOpen = false;
            CloseView();
        }

        public void UseAndSetupNameButton()
        {
            ShouldWeContinue = true;
            ShouldEquipmentViewBeOpen = true;
            CloseView();
        }

        public void CancelButton()
        {
            ShouldWeContinue = false;
            ShouldEquipmentViewBeOpen = false;
            CloseView();
        }

        private void CloseView()
        {
            IsClosed = true;
            TryClose();
        }

    }
}
