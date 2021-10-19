using Caliburn.Micro;

namespace Iit.Fibertest.Client
{
    public class TraceTableRow : PropertyChangedBase
    {
        public int Ordinal { get; set; }

        private string _nodeTitle;
        public string NodeTitle
        {
            get => _nodeTitle;
            set
            {
                if (value == _nodeTitle) return;
                _nodeTitle = value;
                NotifyOfPropertyChange();
            }
        }

        private string _equipmentType = Dto.EquipmentType.EmptyNode.ToString();
        public string EquipmentType
        {
            get => _equipmentType;
            set
            {
                if (value == _equipmentType) return;
                _equipmentType = value;
                NotifyOfPropertyChange();
            }
        }

        private string _equipmentTitle;
        public string EquipmentTitle
        {
            get => _equipmentTitle;
            set
            {
                if (value == _equipmentTitle) return;
                _equipmentTitle = value;
                NotifyOfPropertyChange();
            }
        }

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

        public TraceTableRow Clone()
        {
            return new TraceTableRow()
            {
                Ordinal = Ordinal,
                NodeTitle = NodeTitle,
                EquipmentType = EquipmentType,
                EquipmentTitle = EquipmentTitle,
                GpsCoors = GpsCoors,
            };
        }
    }
}