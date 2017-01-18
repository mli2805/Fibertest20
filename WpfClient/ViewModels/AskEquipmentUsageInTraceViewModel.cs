using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.WpfClient.ViewModels
{
    public class AskEquipmentUsageInTraceViewModel : Screen
    {
        private readonly List<Equipment> _possibleEquipment;
        private readonly bool _isLastNode;
        public bool IsClosed { get; set; }

        public List<MyRadioButton> Choices { get; set; } // for binding

        public bool ShouldWeContinue { get; set; }
        public bool ShouldEquipmentViewBeOpen { get; set; }

        public AskEquipmentUsageInTraceViewModel(List<Equipment> possibleEquipment, bool isLastNode)
        {
            _possibleEquipment = possibleEquipment;
            _isLastNode = isLastNode;
            InitializeChoices();
        }

        private void InitializeChoices()
        {
            Choices = new List<MyRadioButton>();
            foreach (var equipment in _possibleEquipment)
            {
                Choices.Add(new MyRadioButton {Title = equipment.Title, IsSelected = equipment == _possibleEquipment.First()});
            }
            Choices.Add(new MyRadioButton {Title = "Не использовать", IsSelected = false, IsEnabled = !_isLastNode});
        }

        public Guid GetSelectedGuid()
        {
            return GetSelectedRadioButton() == _possibleEquipment.Count ? Guid.Empty : _possibleEquipment[GetSelectedRadioButton()].Id;
        }

        private int GetSelectedRadioButton()
        {
            foreach (var myRadioButton in Choices)
            {
                if (myRadioButton.IsSelected)
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

        public void UseAndChangeButton()
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
