using Caliburn.Micro;
using Iit.Fibertest.Graph;

namespace LicenseMaker
{
    public class LicenseModel : PropertyChangedBase
    {
        private string _owner;
        private int _rtuCount = 1;
        private int _clientStationCount = 1;
        private bool _superClientEnabled;

        public string Owner
        {
            get => _owner;
            set
            {
                if (value == _owner) return;
                _owner = value;
                NotifyOfPropertyChange();
            }
        }

        public int RtuCount
        {
            get => _rtuCount;
            set
            {
                if (value == _rtuCount) return;
                _rtuCount = value;
                NotifyOfPropertyChange();
            }
        }

        public int ClientStationCount
        {
            get => _clientStationCount;
            set
            {
                if (value == _clientStationCount) return;
                _clientStationCount = value;
                NotifyOfPropertyChange();
            }
        }

        public bool SuperClientEnabled
        {
            get => _superClientEnabled;
            set
            {
                if (value == _superClientEnabled) return;
                _superClientEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        public LicenseModel()
        {
        }

        public LicenseModel(License license)
        {
            Owner = license.Owner;
            RtuCount = license.RtuCount;
            ClientStationCount = license.ClientStationCount;
            SuperClientEnabled = license.SuperClientEnabled;
        }
    }
}