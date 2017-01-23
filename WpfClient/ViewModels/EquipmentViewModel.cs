using System;
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

        public RadioButtonModel CableReserve { get; set; } = new RadioButtonModel { Title = "CableReserve", IsChecked = false };
        public RadioButtonModel Sleeve { get; } = new RadioButtonModel { Title = "Sleeve", IsChecked = true };
        public RadioButtonModel Cross { get; } = new RadioButtonModel { Title = "Cross", IsChecked = false };
        public RadioButtonModel Terminal { get; } = new RadioButtonModel { Title = "Terminal", IsChecked = false };
        public RadioButtonModel Other { get; } = new RadioButtonModel { Title = "Other", IsChecked = false };

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
                    var errorNotificationViewModel = new NotificationViewModel(result);
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
            if (CableReserve.IsChecked)
                return EquipmentType.CableReserve;
            if (Sleeve.IsChecked)
                return EquipmentType.Sleeve;
            if (Cross.IsChecked)
                return EquipmentType.Cross;
            if (Terminal.IsChecked)
                return EquipmentType.Terminal;
            //else if (Other.IsSelected)
            return EquipmentType.Other;
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
