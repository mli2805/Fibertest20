using System;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Commands;

namespace Iit.Fibertest.WpfClient.ViewModels
{
    public class RadioButton
    {
        public string Title { get; set; }
        public bool IsSelected { get; set; }
    }
    public class EquipmentViewModel : Screen
    {
        private readonly Guid _nodeIdOnlyForAddEquipmentCase;
        private readonly ReadModel _readModel;
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

        private Guid _equipmentId;

        public RadioButton CableReserve { get; set; } = new RadioButton() {Title = "CableReserve", IsSelected = false};
        public RadioButton Sleeve { get; } = new RadioButton() {Title = "Sleeve", IsSelected = true};
        public RadioButton Cross { get; } = new RadioButton() {Title = "Cross", IsSelected = false};
        public RadioButton Terminal { get; } = new RadioButton() {Title = "Terminal", IsSelected = false};
        public RadioButton Other { get; } = new RadioButton() {Title = "Other", IsSelected = false};

        public bool IsClosed { get; set; }
        public EquipmentViewModel(Guid nodeId, Guid equipmentId, ReadModel readModel, Aggregate aggregate)
        {
            _nodeIdOnlyForAddEquipmentCase = nodeId;
            _readModel = readModel;
            _aggregate = aggregate;
            _equipmentId = equipmentId;

            IsClosed = false;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = _equipmentId == Guid.Empty ? "Добавление" : "Изменение";
        }

        public void Save()
        {
            if (_equipmentId == Guid.Empty)
                _aggregate.When(new AddEquipment()
                {
                    Id = new Guid(),
                    NodeId = _nodeIdOnlyForAddEquipmentCase,
                    Title = _title,
                    Type = GetSelectedRadioButton(),
                    CableReserveLeft = _cableReserveLeft,
                    CableReserveRight = _cableReserveRight,
                    Comment = _comment
                });
            else
                _aggregate.When(new UpdateEquipment() {Id = _equipmentId});

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
