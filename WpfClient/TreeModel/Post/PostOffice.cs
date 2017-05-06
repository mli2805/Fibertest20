using Caliburn.Micro;

namespace Iit.Fibertest.Client
{
    public class PostOffice : PropertyChangedBase
    {
        private object _message;

        public object Message
        {
            get { return _message; }
            set
            {
                if (Equals(value, _message)) return;
                _message = value;
                NotifyOfPropertyChange();
            }
        }
    }
}