using Caliburn.Micro;

namespace Iit.Fibertest.Graph
{
    public class License : PropertyChangedBase
    {
        private string _owner;
        private int _rtuCount;
        private int _clientStationCount;
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

        public string Version { get; set; }
    }
}
