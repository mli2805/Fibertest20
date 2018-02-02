using System;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class CableReserveInfoViewModel : Screen
    {
        private readonly Equipment _equipment;
        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly Guid _nodeId;
        private readonly ViewMode _mode;
        public string Title { get; set; }
        public int CableReserveM { get; set; }
        public string Comment { get; set; }
        public object Command { get; set; }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = @"Cable reserve";
        }

        public CableReserveInfoViewModel(Guid nodeId)
        {
            _nodeId = nodeId;
            _mode = ViewMode.Add;
        }

        public CableReserveInfoViewModel(Equipment equipment, IWcfServiceForClient c2DWcfManager)
        {
            _equipment = equipment;
            Title = equipment.Title;
            CableReserveM = equipment.CableReserveLeft;
            Comment = equipment.Comment;
            _c2DWcfManager = c2DWcfManager;
            _mode = ViewMode.Update;
        }

        public async void Save()
        {
            if (_mode == ViewMode.Update)
            {
                var cmd = new UpdateEquipment()
                {
                    Id = _equipment.Id,
                    Title = Title,
                    Type = EquipmentType.CableReserve,
                    CableReserveLeft = CableReserveM,
                    CableReserveRight = 0,
                    Comment = Comment,
                };
                Command = cmd; // nodeUpdateView take this command to update its equipment table
                await _c2DWcfManager.SendCommandAsObj(cmd);
            }

            if (_mode == ViewMode.Add)
            {
                var cmd = new AddEquipmentIntoNode()
                {
                    Id = Guid.NewGuid(),
                    NodeId = _nodeId,
                    Title = Title,
                    Type = EquipmentType.CableReserve,
                    CableReserveLeft = CableReserveM,
                    CableReserveRight = 0,
                    Comment = Comment,
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
