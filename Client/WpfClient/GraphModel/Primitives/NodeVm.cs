using System;
using Caliburn.Micro;
using GMap.NET;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class NodeVm : PropertyChangedBase
    {
        public Guid Id { get; set; }

        private string _title;
        private PointLatLng _position;
        private FiberState _state;
        private EquipmentType _type;
        private bool _isHighlighted;

        public string Title
        {
            get => _title;
            set
            {
                if (value == _title) return;
                _title = value;
                NotifyOfPropertyChange();
            }
        }

        public EquipmentType Type
        {
            get => _type;
            set
            {
                if (value == _type) return;
                _type = value;
                NotifyOfPropertyChange();
            }
        }

        public FiberState State
        {
            get => _state;
            set
            {
                if (value == _state) return;
                _state = value;
                NotifyOfPropertyChange();
            }
        }

        public PointLatLng Position
        {
            get => _position;
            set
            {
                if (value.Equals(_position)) return;
                _position = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsHighlighted
        {
            get => _isHighlighted;
            set
            {
                if (value == _isHighlighted) return;
                _isHighlighted = value;
                NotifyOfPropertyChange();
            }
        }

        public string Comment { get; set; }
    }
}