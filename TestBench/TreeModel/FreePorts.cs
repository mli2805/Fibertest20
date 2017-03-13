using Caliburn.Micro;

namespace Iit.Fibertest.TestBench
{
    public class FreePorts : PropertyChangedBase
    {
        private bool _areVisible;
        public bool AreVisible
        {
            get { return _areVisible; }
            set
            {
                if (value == _areVisible) return;
                _areVisible = value;
                NotifyOfPropertyChange();
            }
        }
    }
}