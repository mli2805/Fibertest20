using Caliburn.Micro;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class GponModel : PropertyChangedBase
    {
        public TceS Tce { get; set; }
        public int Slot { get; set; }

        public int GponInterface { get; set; }

        private Rtu _rtu;
        public Rtu Rtu
        {
            get => _rtu;
            set
            {
                if (Equals(value, _rtu)) return;
                _rtu = value;
                NotifyOfPropertyChange();
            }
        }

        private Otau _otau;
        public Otau Otau
        {
            get => _otau;
            set
            {
                if (Equals(value, _otau)) return;
                _otau = value;
                NotifyOfPropertyChange();
            }
        }

        private int _otauPort;
        public int OtauPort
        {
            get => _otauPort;
            set
            {
                if (value == _otauPort) return;
                _otauPort = value;
                NotifyOfPropertyChange();
            }
        }

        private string _traceTitle;
        public string TraceTitle
        {
            get => _traceTitle;
            set
            {
                if (value == _traceTitle) return;
                _traceTitle = value;
                NotifyOfPropertyChange();
            }
        }

        public void ClearRelation()
        {
            Rtu = null;
            Otau = null;
            OtauPort = 0;
            TraceTitle = "";
        }
    }
}