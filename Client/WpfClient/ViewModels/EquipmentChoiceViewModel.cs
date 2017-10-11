using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class EquipmentChoiceViewModel : Screen
    {
        private readonly IWindowManager _windowManager;
        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly List<Equipment> _possibleEquipment;
        private readonly string _nodeTitle;
        private readonly bool _isLastNode;
        public bool IsClosed { get; set; }

        public string Explanation { get; set; }
        public string Explanation2 { get; set; }
        public List<RadioButtonModel> Choices { get; set; } // for binding

        public bool ShouldWeContinue { get; set; }
        public bool ShouldEquipmentViewBeOpen { get; set; }

        public EquipmentChoiceViewModel(IWindowManager windowManager, IWcfServiceForClient c2DWcfManager, List<Equipment> possibleEquipment, string nodeTitle, bool isLastNode)
        {
            _windowManager = windowManager;
            _c2DWcfManager = c2DWcfManager;
            _possibleEquipment = possibleEquipment;
            _nodeTitle = nodeTitle;
            _isLastNode = isLastNode;
            InitializeChoices();
        }

        private void InitializeChoices()
        {
            Explanation = Resources.SID_Select_equipment_for_trace;
            Explanation2 = string.Format(Resources.SID_in_node, _nodeTitle);
            Choices = new List<RadioButtonModel>();
            foreach (var equipment in _possibleEquipment)
            {
                var radioButtonModel = new RadioButtonModel
                {
                    Title = string.IsNullOrEmpty(equipment.Title) 
                        ? string.Format(Resources.SID_equipment_without_name, equipment.Type.ToLocalizedString()) 
                        : equipment.Title,
                    IsChecked = equipment == _possibleEquipment.First()
                };
                radioButtonModel.PropertyChanged += RadioButtonModel_PropertyChanged;
                Choices.Add(radioButtonModel);
            }
            var doNotUseModel = new RadioButtonModel {Title = Resources.SID_Do_not_use, IsChecked = false, IsEnabled = !_isLastNode};
            doNotUseModel.PropertyChanged += RadioButtonModel_PropertyChanged;
            Choices.Add(doNotUseModel);
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Select_equipment;
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
            return Choices.IndexOf(Choices.First(c=>c.IsChecked));
        }

        public void SelectButton()
        {
            ShouldWeContinue = true;
            ShouldEquipmentViewBeOpen = false;
            CloseView();
        }

        public void SelectAndSetupNameButton()
        {
            ShouldWeContinue = true;
            var vm = new EquipmentInfoViewModel(_possibleEquipment[GetCheckedRadioButton()], _c2DWcfManager);
            _windowManager.ShowDialog(vm);

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
