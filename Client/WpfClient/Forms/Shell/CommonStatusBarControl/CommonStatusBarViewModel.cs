using Caliburn.Micro;

namespace Iit.Fibertest.Client
{
    public class CommonStatusBarViewModel : PropertyChangedBase
    {
        private string _statusBarMessage2;

        public string StatusBarMessage2
        {
            get { return _statusBarMessage2; }
            set
            {
                if (value == _statusBarMessage2) return;
                _statusBarMessage2 = value;
                NotifyOfPropertyChange();
            }
        }
    }
}
