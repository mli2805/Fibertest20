using Caliburn.Micro;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class ActiveTrace : PropertyChangedBase
    {
        private Trace _trace;

        public Trace Trace  
        {
            get => _trace;
            set
            {
                if (Equals(value, _trace)) return;
                _trace = value;
                NotifyOfPropertyChange();
            }
        }
    }
}
