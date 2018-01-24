using System;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class EquipmentInfoViewModel : Screen
    {
        public Guid EquipmentId { get; set; }
        public Equipment Equipment { get; }
        public Guid NodeId;
        private readonly EquipmentViewMode _mode;
        private readonly IWcfServiceForClient _c2DWcfManager;

        public EquipmentInfoModel Model { get; set; } = new EquipmentInfoModel();

        public bool IsClosed { get; set; }

        public object Command { get; set; }

        // Add
        public EquipmentInfoViewModel(Guid nodeId)
        {
            _mode = EquipmentViewMode.Add;
            NodeId = nodeId;
            Model.SetSelectedRadioButton(EquipmentType.Cross);

            IsClosed = false;
        }

        // Update
        public EquipmentInfoViewModel(Equipment equipment, IWcfServiceForClient c2DWcfManager)
        {
            _mode = EquipmentViewMode.Update;
            Equipment = equipment;
            EquipmentId = equipment.Id;
            NodeId = equipment.NodeId;

            Model.Title = equipment.Title;
            Model.SetSelectedRadioButton(equipment.Type);
            Model.CableReserveLeft = equipment.CableReserveLeft;
            Model.CableReserveRight = equipment.CableReserveRight;
            Model.Comment = equipment.Comment;

            _c2DWcfManager = c2DWcfManager;

            IsClosed = false;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = _mode == EquipmentViewMode.Add ? Resources.SID_Add_Equipment : Resources.SID_Edit_Equipment;
        }

        public async void Save()
        {
            var eqType = Model.GetSelectedRadioButton();

            if (_mode == EquipmentViewMode.Update)
            {
                var cmd = new UpdateEquipment()
                {
                    Id = EquipmentId,
                    Title = Model.Title,
                    Type = eqType,
                    CableReserveLeft = eqType == EquipmentType.CableReserve ? Model.CableReserveM : Model.CableReserveLeft,
                    CableReserveRight = Model.CableReserveRight,
                    Comment = Model.Comment,
                };
                Command = cmd; // nodeUpdateView take this command to update its equipment table
                await _c2DWcfManager.SendCommandAsObj(cmd);
            }

            if (_mode == EquipmentViewMode.Add)
            {
                EquipmentId = Guid.NewGuid();
                var cmd = new AddEquipmentIntoNode()
                {
                    Id = EquipmentId,
                    NodeId = NodeId,
                    Title = Model.Title,
                    Type = eqType,
                    CableReserveLeft = eqType == EquipmentType.CableReserve ? Model.CableReserveM : Model.CableReserveLeft,
                    CableReserveRight = Model.CableReserveRight,
                    Comment = Model.Comment,
                };
                Command = cmd;
                // for equipment addition this part of command 
                // would be OUTSIDE amplified with list of trace which use this equipment 
            }

            CloseView();
        }

        public void Cancel()
        {
            CloseView();
        }

        private void CloseView()
        {
            IsClosed = true;
            TryClose();
        }
    }
}
