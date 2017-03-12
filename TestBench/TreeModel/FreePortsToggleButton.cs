using Caliburn.Micro;

namespace Iit.Fibertest.TestBench
{
    public class FreePortsToggleButton : PropertyChangedBase
    {
        private FreePortsDisplayMode _state;

        public FreePortsDisplayMode State
        {
            get { return _state; }
            set
            {
                if (value == _state) return;
                _state = value;
                NotifyOfPropertyChange();
            }
        }
    }
}