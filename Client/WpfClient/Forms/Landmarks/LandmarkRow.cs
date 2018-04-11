using System;
using Caliburn.Micro;

namespace Iit.Fibertest.Client
{
    public class LandmarkRow : PropertyChangedBase
    {
        public Guid NodeId { get; set; }
        public int Number { get; set; }
        public string NodeTitle { get; set; }
        public string NodeComment { get; set; }
        public Guid EquipmentId { get; set; }
        public string EquipmentTitle { get; set; }
        public string EquipmentType { get; set; }
        public string Distance { get; set; }
        public string EventNumber { get; set; }

        private string _gpsCoors;
        public string GpsCoors
        {
            get => _gpsCoors;
            set
            {
                if (value == _gpsCoors) return;
                _gpsCoors = value;
                NotifyOfPropertyChange();
            }
        }
    }
}