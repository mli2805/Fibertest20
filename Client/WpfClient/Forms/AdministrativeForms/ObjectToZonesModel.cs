using System;
using System.Collections.Generic;
using System.ComponentModel;
using Caliburn.Micro;

namespace Iit.Fibertest.Client
{
    public class BoolWithNotification : PropertyChangedBase
    {
        private bool _isChecked;
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (value == _isChecked) return;
                _isChecked = value;
                NotifyOfPropertyChange();
            }
        }
    }
    public class ObjectToZonesModel : PropertyChangedBase
    {
        public string ObjectTitle { get; set; }
        public Guid ObjectId { get; set; }

        private bool _isRtu;

        public bool IsRtu
        {
            get { return _isRtu; }
            set
            {
                if (value == _isRtu) return;
                _isRtu = value;
                NotifyOfPropertyChange();
            }
        }

        public bool[] Zones { get; set; }

        private List<BoolWithNotification> _isInZones = new List<BoolWithNotification>();
        public List<BoolWithNotification> IsInZones
        {
            get { return _isInZones; }
            set
            {
                if (Equals(value, _isInZones)) return;
                _isInZones = value;
                NotifyOfPropertyChange();
            }
        }


        public ObjectToZonesModel()
        {
        }

        public ObjectToZonesModel(int maxZonesCount)
        {
            Zones = new bool[maxZonesCount];
        }
    }
}