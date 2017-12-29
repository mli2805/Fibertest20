using Caliburn.Micro;

namespace Iit.Fibertest.Client
{
    public class CommonStatusBarViewModel : PropertyChangedBase
    {
        private string _statusBarMessage1 = "BBBB";
        private string _statusBarMessage2 = "AAA";

        public string StatusBarMessage1
        {
            get { return _statusBarMessage1; }
            set
            {
                if (value == _statusBarMessage1) return;
                _statusBarMessage1 = value;
                NotifyOfPropertyChange();
            }
        }

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
