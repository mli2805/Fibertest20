using Caliburn.Micro;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class OtdrParametersTemplate : PropertyChangedBase
    {
        public int Id;
        public string Title { get; set; }
        public string Lmax;
        public string Dl;
        public string Tp;
        public string Time;
        private bool _isChecked;
        public string Description => Id == 0 
            ? Resources.SID_Automatic_detection_of_measurement_parameters 
            : F();

        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (value == _isChecked) return;
                _isChecked = value;
                NotifyOfPropertyChange();
            }
        }

        private string F()
        {
            var km = Resources.SID_km;
            var m = Resources.SID_m;
            var ns = Resources.SID_ns;
            var minsec = Resources.SID_min_sec;
            return $@"Lmax = {Lmax} {km};   dL = {Dl} {m};   Tp = {Tp} {ns};   t = {Time} {minsec}";
        }
    }
}