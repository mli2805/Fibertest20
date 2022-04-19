using System.ComponentModel;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class GponModel : PropertyChangedBase, IDataErrorInfo
    {
        public TceS Tce { get; set; }
        public int SlotPosition { get; set; }

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

        private string _otauPortNumberStr;
        public string OtauPortNumberStr
        {
            get => _otauPortNumberStr;
            set
            {
                if (value == _otauPortNumberStr) return;
                _otauPortNumberStr = value;
                NotifyOfPropertyChange();
            }
        }

        private Trace _trace;
        public Trace Trace
        {
            get => _trace;
            set
            {
                if (Equals(value, _trace)) return;
                _trace = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(TraceTitle));
            }
        }


        public string TraceTitle => Trace?.Title ?? "";
        

        public void ClearRelation()
        {
            Rtu = null;
            Otau = null;
            OtauPortNumberStr = "";
            Trace = null;
        }

        public string this[string columnName]
        {
            get
            {
                var errorMessage = string.Empty;
                switch (columnName)
                {
                    case "OtauPortDto":
                        if (string.IsNullOrEmpty(OtauPortNumberStr))
                            break;
                        if (!int.TryParse(OtauPortNumberStr, out int port))
                            errorMessage = Resources.SID_Invalid_input;
                        if (port < 1)
                            errorMessage = Resources.SID_Invalid_input;
                        Error = errorMessage;
                        break;
                }
                return errorMessage;
            }
        }

        public string Error { get; set; }
    }
  
}