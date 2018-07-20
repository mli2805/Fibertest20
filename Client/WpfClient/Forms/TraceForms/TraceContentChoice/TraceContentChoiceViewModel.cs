using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForClientInterface;
using Iit.Fibertest.WpfCommonViews;


namespace Iit.Fibertest.Client
{
    public class TraceContentChoiceViewModel : Screen
    {
        private readonly ILifetimeScope _globalScope;
        private readonly IniFile _iniFile;
        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly IWindowManager _windowManager;
        private readonly EquipmentOfChoiceModelFactory _equipmentOfChoiceModelFactory;
        private List<Equipment> _possibleEquipment;
        private Node _node;
        public string NodeTitle { get; set; }
        public List<EquipmentOfChoiceModel> EquipmentChoices { get; set; }
        public EquipmentOfChoiceModel NoEquipmentInNodeChoice { get; set; }

        private Visibility _leftAndRightVisibility = Visibility.Collapsed;
        public Visibility LeftAndRightVisibility
        {
            get { return _leftAndRightVisibility; }
            set
            {
                if (value == _leftAndRightVisibility) return;
                _leftAndRightVisibility = value;
                NotifyOfPropertyChange();
            }
        }


        public bool ShouldWeContinue { get; set; }

        public TraceContentChoiceViewModel(ILifetimeScope globalScope, IniFile iniFile, IWcfServiceForClient c2DWcfManager,
            IWindowManager windowManager, EquipmentOfChoiceModelFactory equipmentOfChoiceModelFactory)
        {
            _globalScope = globalScope;
            _iniFile = iniFile;
            _c2DWcfManager = c2DWcfManager;
            _windowManager = windowManager;
            _equipmentOfChoiceModelFactory = equipmentOfChoiceModelFactory;
        }

        public void Initialize(List<Equipment> possibleEquipment, Node node, bool isLastNode)
        {
            _node = node;
            NodeTitle = node.Title;

            _possibleEquipment = possibleEquipment;
            EquipmentChoices = new List<EquipmentOfChoiceModel>();
            foreach (var equipment in possibleEquipment.Where(e => e.Type > EquipmentType.EmptyNode))
            {
                var equipmentOfChoiceModel = _equipmentOfChoiceModelFactory.Create(equipment);
                equipmentOfChoiceModel.IsSelected = equipment == possibleEquipment.First();
                equipmentOfChoiceModel.PropertyChanged += EquipmentOfChoiceModel_PropertyChanged;
                EquipmentChoices.Add(equipmentOfChoiceModel);
            }
            if (EquipmentChoices.Any()) LeftAndRightVisibility = Visibility.Visible;


            var emptyNode = possibleEquipment.Single(e => e.Type == EquipmentType.EmptyNode);
            NoEquipmentInNodeChoice = _equipmentOfChoiceModelFactory.CreateDoNotUseEquipment(emptyNode.EquipmentId, isLastNode);
            NoEquipmentInNodeChoice.PropertyChanged += EquipmentOfChoiceModel_PropertyChanged;

            if (EquipmentChoices.Any())
                EquipmentChoices[0].IsSelected = true;
            else
                NoEquipmentInNodeChoice.IsSelected = true;
        }


        private void EquipmentOfChoiceModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            EquipmentOfChoiceModel model = (EquipmentOfChoiceModel)sender;
            if (e.PropertyName == @"IsSelected" && model.IsSelected)
            {
                foreach (var mo in EquipmentChoices.Where(m => m != model))
                    mo.IsSelected = false;
                if (NoEquipmentInNodeChoice != model)
                    NoEquipmentInNodeChoice.IsSelected = false;
            }
        }

        public Guid GetSelectedEquipmentGuid()
        {
            foreach (var mo in EquipmentChoices)
                if (mo.IsSelected)
                    return mo.EquipmentId;
            return NoEquipmentInNodeChoice.EquipmentId;
        }

        public string GetSelectedDualName()
        {
            var result = NodeTitle;
            var selectedModel = EquipmentChoices.FirstOrDefault(m => m.IsSelected);
            if (selectedModel == null) return result;
            result = result + @" / " + selectedModel.TitleOfEquipment;// even if equipment title is empty
            return result == @" / " ? null : result;
        }


        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Trace_components_selection;
        }

        public async void NextButton()
        {
            using (_globalScope.Resolve<IWaitCursor>())
            {
                ShouldWeContinue = true;
                var maxCableReserve = _iniFile.Read(IniSection.Miscellaneous, IniKey.MaxCableReserve, 200);

                if (_node.Title != NodeTitle)
                    await SendNodeTitle();

                foreach (var equipment in _possibleEquipment.Where(e => e.Type != EquipmentType.EmptyNode))
                {
                    var model = EquipmentChoices.FirstOrDefault(m => m.EquipmentId == equipment.EquipmentId);
                    if (model == null) continue;

                    if (equipment.Title != model.TitleOfEquipment ||
                        equipment.CableReserveLeft != model.LeftCableReserve ||
                        equipment.CableReserveRight != model.RightCableReserve)
                    {
                        if (model.LeftCableReserve > maxCableReserve || model.RightCableReserve > maxCableReserve)
                        {
                            var vm = new MyMessageBoxViewModel(MessageType.Error, string.Format(Resources.SID_Cable_reserve_could_not_be_more_than__0__m, maxCableReserve));
                            _windowManager.ShowDialogWithAssignedOwner(vm);
                            return;
                        }
                        await SendEquipmentChanges(equipment, model.TitleOfEquipment, model.LeftCableReserve, model.RightCableReserve);
                    }
                }

                TryClose();
            }
        }

        private async Task<string> SendNodeTitle()
        {
            var cmd = new UpdateNode
            {
                NodeId = _node.NodeId,
                Title = NodeTitle.Trim(),
                Comment = _node.Comment,
            };
            return await _c2DWcfManager.SendCommandAsObj(cmd);
        }

        private async Task<string> SendEquipmentChanges(Equipment equipment, string newTitle, int leftCableReserve, int rightCableReserve)
        {
            var cmd = new UpdateEquipment()
            {
                EquipmentId = equipment.EquipmentId,
                Title = newTitle,
                Type = equipment.Type,
                CableReserveLeft = leftCableReserve,
                CableReserveRight = rightCableReserve,
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
