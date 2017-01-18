﻿using System;
using System.Collections.Generic;
using AutoMapper;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Commands;

namespace Iit.Fibertest.WpfClient.ViewModels
{
    public class EquipmentViewModel : Screen
    {
        private readonly IWindowManager _windowManager;
        private readonly Guid _nodeIdOnlyForAddEquipmentCase;
        private readonly Aggregate _aggregate;
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

        public List<Guid> TracesForInsertion { get; set; }

        public Guid EquipmentId { get; set; }

        public MyRadioButton CableReserve { get; set; } = new MyRadioButton() { Title = "CableReserve", IsSelected = false };
        public MyRadioButton Sleeve { get; } = new MyRadioButton() { Title = "Sleeve", IsSelected = true };
        public MyRadioButton Cross { get; } = new MyRadioButton() { Title = "Cross", IsSelected = false };
        public MyRadioButton Terminal { get; } = new MyRadioButton() { Title = "Terminal", IsSelected = false };
        public MyRadioButton Other { get; } = new MyRadioButton() { Title = "Other", IsSelected = false };

        public bool IsClosed { get; set; }
        public EquipmentViewModel(IWindowManager windowManager, Guid nodeId, Guid equipmentId, List<Guid> tracesForInsertion, Aggregate aggregate)
        {
            _windowManager = windowManager;
            _nodeIdOnlyForAddEquipmentCase = nodeId;
            _aggregate = aggregate;
            EquipmentId = equipmentId;
            TracesForInsertion = tracesForInsertion;

            IsClosed = false;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = EquipmentId == Guid.Empty ? "Добавление" : "Изменение";
        }

        public void Save()
        {
            IMapper mapper = new MapperConfiguration(
              cfg => cfg.AddProfile<MappingViewModeltoDomainEntity>()).CreateMapper();

            if (EquipmentId == Guid.Empty) // добавление нового оборудования
            {
                EquipmentId = Guid.NewGuid();
                var cmd = mapper.Map<AddEquipment>(this);
                cmd.Id = EquipmentId;
                cmd.NodeId = _nodeIdOnlyForAddEquipmentCase;
                cmd.TracesForInsertion = TracesForInsertion;
                var result = _aggregate.When(cmd);
                if (result != null)
                {
                    var errorNotificationViewModel = new ErrorNotificationViewModel(result);
                    _windowManager.ShowDialog(errorNotificationViewModel);
                    return;
                }
            }
            else  // редактирование существовавшего
            {
                var cmd = mapper.Map<UpdateEquipment>(this);
                cmd.Id = EquipmentId;
                _aggregate.When(cmd);
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
            if (CableReserve.IsSelected)
                return EquipmentType.CableReserve;
            else if (Sleeve.IsSelected)
                return EquipmentType.Sleeve;
            else if (Cross.IsSelected)
                return EquipmentType.Cross;
            else if (Terminal.IsSelected)
                return EquipmentType.Terminal;
            //else if (Other.IsSelected)
            return EquipmentType.Other;
        }
        private void SetSelectedRadioButton(EquipmentType type)
        {
            CleanSelectedRadioButton();
            if (type == EquipmentType.CableReserve)
                CableReserve.IsSelected = true;
            else if (type == EquipmentType.Sleeve)
                Sleeve.IsSelected = true;
            else if (type == EquipmentType.Cross)
                Cross.IsSelected = true;
            else if (type == EquipmentType.Terminal)
                Terminal.IsSelected = true;
            else if (type == EquipmentType.Other)
                Other.IsSelected = true;
        }

        private void CleanSelectedRadioButton()
        {
            CableReserve.IsSelected = false;
            Sleeve.IsSelected = false;
            Cross.IsSelected = false;
            Terminal.IsSelected = false;
            Other.IsSelected = false;
        }
    }
}
