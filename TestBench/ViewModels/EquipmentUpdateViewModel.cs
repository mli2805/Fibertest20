﻿using System;
using AutoMapper;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.TestBench
{
    public class EquipmentUpdateViewModel : Screen
    {
        public Guid NodeId;
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
        public RadioButtonModel Sleeve { get; } = new RadioButtonModel() { Title = Resources.SID_Sleeve };
        public RadioButtonModel Cross { get; } = new RadioButtonModel() { Title = Resources.SID_Cross };
        public RadioButtonModel Terminal { get; } = new RadioButtonModel() { Title = Resources.SID_Terminal };
        public RadioButtonModel Other { get; } = new RadioButtonModel() { Title = Resources.SID_Other };

        public bool IsClosed { get; set; }
        public bool IsSaveEnabled => GetSelectedRadioButton() != EquipmentType.None;

        public object Command { get; set; }


        public EquipmentUpdateViewModel(Guid nodeId, Guid equipmentId, Bus bus)
        {
            _bus = bus;
            NodeId = nodeId;
            EquipmentId = equipmentId;

            IsClosed = false;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = EquipmentId == Guid.Empty ? Resources.SID_Add_Equipment : Resources.SID_Edit_Equipment;
        }

        public void Save()
        {
            IMapper mapper = new MapperConfiguration(
              cfg => cfg.AddProfile<MappingViewModelToCommand>()).CreateMapper();

            if (EquipmentId == Guid.Empty) // добавление нового оборудования
            {
                EquipmentId = Guid.NewGuid();
                var cmd = mapper.Map<AddEquipmentIntoNode>(this);
                cmd.Id = EquipmentId;
                cmd.NodeId = NodeId;
                Command = cmd;
                // for equipment addition this part of command 
                // would be amplified with list of trace which use this equipment 
            }
            else  // редактирование существовавшего
            {
                var cmd = mapper.Map<UpdateEquipment>(this);
                cmd.Id = EquipmentId;
                _bus.SendCommand(cmd);
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
                return EquipmentType.Sleeve;
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
            else if (type == EquipmentType.Sleeve)
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
