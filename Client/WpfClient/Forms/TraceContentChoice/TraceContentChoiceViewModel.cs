using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class TraceContentChoiceViewModel : Screen
    {
        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly EquipmentOfChoiceModelFactory _equipmentOfChoiceModelFactory;
        private List<Equipment> _possibleEquipment;
        private string _nodeTitle;
        public string NameOfNode { get; set; }
        public List<EquipmentOfChoiceModel> Choices { get; set; }
        public bool ShouldWeContinue { get; set; }

        public TraceContentChoiceViewModel(IWcfServiceForClient c2DWcfManager, EquipmentOfChoiceModelFactory equipmentOfChoiceModelFactory)
        {
            _c2DWcfManager = c2DWcfManager;
            _equipmentOfChoiceModelFactory = equipmentOfChoiceModelFactory;
        }

        public void Initialize(List<Equipment> possibleEquipment, string nodeTitle, bool isLastNode)
        {
            _nodeTitle = nodeTitle;
            NameOfNode = nodeTitle;

            _possibleEquipment = possibleEquipment;
            Choices = new List<EquipmentOfChoiceModel>();
            foreach (var equipment in possibleEquipment.Where(e=>e.Type != EquipmentType.EmptyNode))
            {
                var equipmentOfChoiceModel = _equipmentOfChoiceModelFactory.Create(equipment);
                equipmentOfChoiceModel.IsSelected = equipment == possibleEquipment.First();
                equipmentOfChoiceModel.PropertyChanged += EquipmentOfChoiceModel_PropertyChanged;
                Choices.Add(equipmentOfChoiceModel);
            }

            // just because this option should be the last
            var emptyNode = possibleEquipment.Single(e => e.Type == EquipmentType.EmptyNode);
            var doNotUseOptionModel = _equipmentOfChoiceModelFactory.Create(emptyNode.Id, isLastNode);
            doNotUseOptionModel.PropertyChanged += EquipmentOfChoiceModel_PropertyChanged;
            Choices.Add(doNotUseOptionModel);
        }

       
        private void EquipmentOfChoiceModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            EquipmentOfChoiceModel model = (EquipmentOfChoiceModel) sender;
            if (e.PropertyName == "IsSelected" && model.IsSelected)
                foreach (var mo in Choices.Where(m=>m != model))
                    mo.IsSelected = false;
        }

        public Guid GetSelectedEquipmentGuid()
        {
            return Choices.First(c=>c.IsSelected).EquipmentId;
        }


        protected override void OnViewLoaded(object view)
        {
            DisplayName = "";
        }

        public void NextButton()
        {
            ShouldWeContinue = true;
            if (_nodeTitle != NameOfNode)
                SendNodeTitle();
            foreach (var equipment in _possibleEquipment.Where(e=>e.Type != EquipmentType.EmptyNode))
            {
                var model = Choices.First(m => m.EquipmentId == equipment.Id);
                if (equipment.Title != model.NameOfEquipment)
                    SendEquipmentTitle();
            }
            TryClose();
        }

        private void SendNodeTitle()
        {

        }

        private void SendEquipmentTitle()
        {

        }

        public void CancelButton()
        {
            ShouldWeContinue = false;
            TryClose();
        }
    }
}
