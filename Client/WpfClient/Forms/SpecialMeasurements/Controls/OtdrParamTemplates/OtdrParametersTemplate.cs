using Caliburn.Micro;

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
        public string Description => $@"Lmax = {Lmax} km;   dL = {Dl} m;   Tp = {Tp} ns;   t = {Time} min:sec";

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
    }
}