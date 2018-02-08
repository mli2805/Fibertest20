using System;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class CableReserveInfoViewModel : Screen
    {
        private readonly IniFile _iniFile;
        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly IWindowManager _windowManager;
        private ViewMode _mode;
        private Equipment _equipment;
        private Guid _nodeId;
        public string Title { get; set; }
        public int CableReserveM { get; set; }
        public string Comment { get; set; }
        public object Command { get; set; }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = @"Cable reserve";
        }

        public CableReserveInfoViewModel(IniFile iniFile, IWcfServiceForClient c2DWcfManager, IWindowManager windowManager)
        {
            _iniFile = iniFile;
            _c2DWcfManager = c2DWcfManager;
            _windowManager = windowManager;
        }

        public void InitializeForAdd(Guid nodeId)
        {
            _nodeId = nodeId;
            _mode = ViewMode.Add;
        }

        public void InitializeForUpdate(Equipment equipment)
        {
            _equipment = equipment;
            Title = equipment.Title;
            CableReserveM = equipment.CableReserveLeft;
            Comment = equipment.Comment;
            _mode = ViewMode.Update;
        }

        public async void Save()
        {
            var maxCableReserve = _iniFile.Read(IniSection.Miscellaneous, IniKey.MaxCableReserve, 200);
            if (CableReserveM > maxCableReserve)
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, string.Format(Resources.SID_Cable_reserve_could_not_be_more_than__0__m, maxCableReserve));
                _windowManager.ShowDialogWithAssignedOwner(vm);
                return;
            }

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
