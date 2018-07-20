using System;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForClientInterface;
using Iit.Fibertest.WpfCommonViews;


namespace Iit.Fibertest.Client
{
    public class EquipmentInfoViewModel : Screen
    {
        public Guid EquipmentId { get; set; }
        public Equipment Equipment { get; set; }
        public Guid NodeId;
        private ViewMode _mode;
        private readonly IniFile _iniFile;
        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly IWindowManager _windowManager;

        public EquipmentInfoModel Model { get; set; } = new EquipmentInfoModel();

        public object Command { get; set; }

        public EquipmentInfoViewModel(IniFile iniFile, IWcfServiceForClient c2DWcfManager, IWindowManager windowManager)
        {
            _iniFile = iniFile;
            _c2DWcfManager = c2DWcfManager;
            _windowManager = windowManager;
        }

        public void InitializeForAdd(Guid nodeId)
        {
            _mode = ViewMode.Add;
            NodeId = nodeId;
            Model.SetSelectedRadioButton(EquipmentType.Closure);
            Model.IsRightCableReserveEnabled = true;
        }

        public void InitializeForUpdate(Equipment equipment)
        {
            _mode = ViewMode.Update;
            Equipment = equipment;
            EquipmentId = equipment.EquipmentId;
            NodeId = equipment.NodeId;

            Model.Title = equipment.Title;
            Model.SetSelectedRadioButton(equipment.Type);
            Model.CableReserveLeft = equipment.CableReserveLeft;
            Model.CableReserveRight = equipment.CableReserveRight;
            Model.Comment = equipment.Comment;

            Model.IsRightCableReserveEnabled = equipment.Type != EquipmentType.Terminal &&
                                               equipment.Type != EquipmentType.CableReserve;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = _mode == ViewMode.Add ? Resources.SID_Add_Equipment : Resources.SID_Edit_Equipment;
        }

        public async void Save()
        {
            var eqType = Model.GetSelectedRadioButton();
            var maxCableReserve = _iniFile.Read(IniSection.Miscellaneous, IniKey.MaxCableReserve, 200);
            if (Model.CableReserveLeft > maxCableReserve || Model.CableReserveRight > maxCableReserve)
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, string.Format(Resources.SID_Cable_reserve_could_not_be_more_than__0__m, maxCableReserve));
                _windowManager.ShowDialogWithAssignedOwner(vm);
                return;
            }
            if (Model.Type == EquipmentType.Terminal && Model.CableReserveRight > 0)
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, @"Запас кабеля после оконечного кросса не имеет смысла");
                _windowManager.ShowDialogWithAssignedOwner(vm);
                return;
            }

            if (_mode == ViewMode.Update)
            {
                var cmd = new UpdateEquipment()
                {
                    EquipmentId = EquipmentId,
                    Title = Model.Title,
                    Type = eqType,
                    CableReserveLeft = Model.CableReserveLeft,
                    CableReserveRight = Model.CableReserveRight,
                    Comment = Model.Comment,
                };
                Command = cmd; // nodeUpdateView take this command to update its equipment table
                await _c2DWcfManager.SendCommandAsObj(cmd);
            }

            if (_mode == ViewMode.Add)
            {
                EquipmentId = Guid.NewGuid();
                var cmd = new AddEquipmentIntoNode()
                {
                    EquipmentId = EquipmentId,
                    NodeId = NodeId,
                    Title = Model.Title,
                    Type = eqType,
                    CableReserveLeft = Model.CableReserveLeft,
                    CableReserveRight = Model.CableReserveRight,
                    Comment = Model.Comment,
                };
                Command = cmd;
                // for equipment addition this part of command 
                // would be OUTSIDE amplified with list of trace which use this equipment 
            }

            TryClose();
        }

        public void Cancel()
        {
            TryClose();
        }

    }
}
