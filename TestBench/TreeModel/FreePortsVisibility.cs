using Caliburn.Micro;

namespace Iit.Fibertest.TestBench
{
    public class FreePortsVisibility : PropertyChangedBase
    {
        private FreePortsVisibilityState _state;

        public FreePortsVisibilityState State
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