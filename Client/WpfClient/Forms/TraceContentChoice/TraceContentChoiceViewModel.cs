using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class TraceContentChoiceViewModel : Screen
    {
        private readonly ILifetimeScope _globalScope;
        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly EquipmentOfChoiceModelFactory _equipmentOfChoiceModelFactory;
        private List<Equipment> _possibleEquipment;
        private Node _node;
        public string NameOfNode { get; set; }
        public List<EquipmentOfChoiceModel> Choices { get; set; }
        public bool ShouldWeContinue { get; set; }

        public TraceContentChoiceViewModel(ILifetimeScope globalScope, IWcfServiceForClient c2DWcfManager, 
            EquipmentOfChoiceModelFactory equipmentOfChoiceModelFactory)
        {
            _globalScope = globalScope;
            _c2DWcfManager = c2DWcfManager;
            _equipmentOfChoiceModelFactory = equipmentOfChoiceModelFactory;
        }

        public void Initialize(List<Equipment> possibleEquipment, Node node, bool isLastNode)
        {
            _node = node;
            NameOfNode = node.Title;

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
            DisplayName = Resources.SID_Trace_equipment_selection;
        }

        public async void NextButton()
        {
            using (_globalScope.Resolve<IWaitCursor>())
            {
                ShouldWeContinue = true;

                if (_node.Title != NameOfNode)
                    await SendNodeTitle();

                foreach (var equipment in _possibleEquipment.Where(e => e.Type != EquipmentType.EmptyNode))
                {
                    var model = Choices.First(m => m.EquipmentId == equipment.Id);
                    if (equipment.Title != model.NameOfEquipment)
                        await SendEquipmentTitle(equipment, model.NameOfEquipment);
                }

                TryClose();
            }
        }

        private async Task<string> SendNodeTitle()
        {
            var cmd = new UpdateNode
            {
                Id = _node.Id,
                Title = NameOfNode.Trim(),
                Comment = _node.Comment,
            };
            return await _c2DWcfManager.SendCommandAsObj(cmd);
        }

        private async Task<string> SendEquipmentTitle(Equipment equipment, string newTitle)
        {
            var cmd = new UpdateEquipment()
            {
                Id = equipment.Id,
                Title = newTitle,
                Type = equipment.Type,
                CableReserveLeft = equipment.CableReserveLeft,
                CableReserveRight = equipment.CableReserveRight,
                Comment = equipment.Comment,
            };
            return await _c2DWcfManager.SendCommandAsObj(cmd);
        }

        public void CancelButton()
        {
            ShouldWeContinue = false;
            TryClose();
        }
    }
}
