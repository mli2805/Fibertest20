using System;
using AutoMapper;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public enum EquipmentViewMode { Add, Update}
    public class EquipmentInfoViewModel : Screen
    {
        public Equipment Equipment { get; }
        public Guid NodeId;
        private readonly EquipmentViewMode _mode;
        private readonly Bus _bus;
        private string _title;
        private int _cableReserveLeft;
        private int _cableReserveRight;
        private string _comment;

        public string Title
        {
            get { return _title; }
            set
            {
                if (value == _title) return;
                _title = value;
                NotifyOfPropertyChange();
            }
        }

        public int CableReserveLeft
        {
            get { return _cableReserveLeft; }
            set
            {
                if (value == _cableReserveLeft) return;
                _cableReserveLeft = value;
                NotifyOfPropertyChange();
            }
        }

        public int CableReserveRight
        {
            get { return _cableReserveRight; }
            set
            {
                if (value == _cableReserveRight) return;
                _cableReserveRight = value;
                NotifyOfPropertyChange();
            }
        }

        public EquipmentType Type
        {
            get { return GetSelectedRadioButton(); }
            set
            {
                SetSelectedRadioButton(value);
            }
        }

        public string Comment
        {
            get { return _comment; }
            set
            {
                if (value == _comment) return;
                _comment = value;
                NotifyOfPropertyChange();
            }
        }

        public Guid EquipmentId { get; set; }

        public RadioButtonModel CableReserve { get; } = new RadioButtonModel() {Title = Resources.SID_CableReserve };
        public RadioButtonModel Sleeve { get; } = new RadioButtonModel() { Title = Resources.SID_Closure };
        public RadioButtonModel Cross { get; } = new RadioButtonModel() { Title = Resources.SID_Cross };
        public RadioButtonModel Terminal { get; } = new RadioButtonModel() { Title = Resources.SID_Terminal };
        public RadioButtonModel Other { get; } = new RadioButtonModel() { Title = Resources.SID_Other };

        public bool IsClosed { get; set; }
        public bool IsSaveEnabled => GetSelectedRadioButton() != EquipmentType.None;

        public object Command { get; set; }


        public EquipmentInfoViewModel(Guid nodeId)
        {
            _mode = EquipmentViewMode.Add;
            NodeId = nodeId;
            Type = EquipmentType.Cross;

            IsClosed = false;
        }

        public EquipmentInfoViewModel(Equipment equipment, Bus bus)
        {
            _mode = EquipmentViewMode.Update;
            Equipment = equipment;
            EquipmentId = equipment.Id;
            NodeId = equipment.NodeId;
            Type = equipment.Type;
            _bus = bus;

            IsClosed = false;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = _mode == EquipmentViewMode.Add ? Resources.SID_Add_Equipment : Resources.SID_Edit_Equipment;
        }

        public void Save()
        {
            IMapper mapper = new MapperConfiguration(
              cfg => cfg.AddProfile<MappingViewModelToCommand>()).CreateMapper();

            if (_mode == EquipmentViewMode.Update)
            {
                var cmd = mapper.Map<UpdateEquipment>(this);
                cmd.Id = EquipmentId;
                _bus.SendCommand(cmd);
                Command = cmd; // nodeUpdateView take this command to update its equipment table
            }

            if (_mode == EquipmentViewMode.Add)
            {
                EquipmentId = Guid.NewGuid();
                var cmd = mapper.Map<AddEquipmentIntoNode>(this);
                cmd.Id = EquipmentId;
                cmd.NodeId = NodeId;
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

        private EquipmentType GetSelectedRadioButton()
        {
            if (CableReserve.IsChecked)
                return EquipmentType.CableReserve;
            if (Sleeve.IsChecked)
                return EquipmentType.Closure;
            if (Cross.IsChecked)
                return EquipmentType.Cross;
            if (Terminal.IsChecked)
                return EquipmentType.Terminal;
            if (Other.IsChecked)
                return EquipmentType.Other;
            return EquipmentType.None;
        }
        private void SetSelectedRadioButton(EquipmentType type)
        {
            CleanSelectedRadioButton();
            if (type == EquipmentType.CableReserve)
                CableReserve.IsChecked = true;
            else if (type == EquipmentType.Closure)
                Sleeve.IsChecked = true;
            else if (type == EquipmentType.Cross)
                Cross.IsChecked = true;
            else if (type == EquipmentType.Terminal)
                Terminal.IsChecked = true;
            else if (type == EquipmentType.Other)
                Other.IsChecked = true;
        }

        private void CleanSelectedRadioButton()
        {
            CableReserve.IsChecked = false;
            Sleeve.IsChecked = false;
            Cross.IsChecked = false;
            Terminal.IsChecked = false;
            Other.IsChecked = false;
        }
    }
}
