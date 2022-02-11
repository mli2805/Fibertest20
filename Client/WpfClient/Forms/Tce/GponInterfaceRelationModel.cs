using Caliburn.Micro;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class GponInterfaceRelationModel : PropertyChangedBase
    {
        private string _rtuTitle;
        public int GponInterface { get; set; }

        public string RtuTitle
        {
            get => _rtuTitle;
            set
            {
                if (value == _rtuTitle) return;
                _rtuTitle = value;
                NotifyOfPropertyChange();
            }
        }

        public Rtu Rtu { get; set; }

        public string OtauTitle { get; set; }
        public int OtauPort { get; set; }
        public string TraceTitle { get; set; } = "";
    }
}