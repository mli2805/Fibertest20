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
        public FiberState State { get; set; }

        public PointLatLng Position { get; set; }
        public string Comment { get; set; }
    }
}