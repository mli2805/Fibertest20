using System;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Commands;

namespace Iit.Fibertest.WpfClient.ViewModels
{
    public struct RadioButton
    {
        public string Title { get; set; }
        public bool IsSelected { get; set; }
    }
    public class AddEquipmentViewModel : Screen
    {
        private readonly Guid _nodeId;
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

        public RadioButton CableReserve { get; } = new RadioButton() {Title = "CableReserve", IsSelected = false};
        public RadioButton Sleeve { get; } = new RadioButton() {Title = "Sleeve", IsSelected = true};
        public RadioButton Cross { get; } = new RadioButton() {Title = "Cross", IsSelected = false};
        public RadioButton Terminal { get; } = new RadioButton() {Title = "Terminal", IsSelected = false};
        public RadioButton Other { get; } = new RadioButton() {Title = "Other", IsSelected = false};

        public bool IsClosed { get; set; }
        public AddEquipmentViewModel(Guid nodeId, ReadModel readModel, Aggregate aggregate)
        {
            _nodeId = nodeId;
            _readModel = readModel;
            _aggregate = aggregate;

            IsClosed = false;
        }

        public void Save()
        {
            _aggregate.When(new AddEquipment()
            {
                Id = new Guid(),
                NodeId = _nodeId,
                Title = _title,
                Type = GetSelectedRadioButton(),
                CableReserveLeft = _cableReserveLeft,
                CableReserveRight = _cableReserveRight,
                Comment = _comment
            });

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
    }
}
