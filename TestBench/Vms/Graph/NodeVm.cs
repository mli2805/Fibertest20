using System;
using Caliburn.Micro;
using GMap.NET;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.TestBench
{
    public class NodeVm : PropertyChangedBase
    {
        public Guid Id { get; set; }

        private string _title;
        private PointLatLng _position;
        private FiberState _state;

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

        public EquipmentType Type { get; set; }

        public FiberState State
        {
            get { return _state; }
            set
            {
                if (value == _state) return;
                _state = value;
                NotifyOfPropertyChange();
            }
        }

        public PointLatLng Position
        {
            get { return _position; }
            set
            {
                if (value.Equals(_position)) return;
                _position = value;
                NotifyOfPropertyChange();
            }
        }

        public string Comment { get; set; }
    }
}