using Caliburn.Micro;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class RelationOfGponModel : PropertyChangedBase
    {
        public Tce Tce { get; set; }
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

        public int OtauPort { get; set; }
        public string TraceTitle { get; set; }
    }
}