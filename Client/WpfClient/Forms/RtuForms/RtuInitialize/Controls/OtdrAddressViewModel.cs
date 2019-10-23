using Caliburn.Micro;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    
    public class OtdrAddressViewModel : PropertyChangedBase
    {
        private string _otdrAddress;
        private int _port;

        public string OtdrAddress
        {
            get => _otdrAddress;
            set
            {
                if (value == _otdrAddress) return;
                _otdrAddress = value;
                NotifyOfPropertyChange();
            }
        }

        public int Port
        {
            get => _port;
            set
            {
                if (value == _port) return;
                _port = value;
                NotifyOfPropertyChange();
            }
        }

        public void FromRtu(Rtu rtu)
        {
            OtdrAddress = rtu.OtdrNetAddress.IsAddressSetAsIp
                ? rtu.OtdrNetAddress.Ip4Address
                : rtu.OtdrNetAddress.HostName;
            Port = rtu.OtdrNetAddress.Port;
        }
    }
}
