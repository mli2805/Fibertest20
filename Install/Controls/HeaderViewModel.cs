using Caliburn.Micro;

namespace Iit.Fibertest.Install
{
    public class HeaderViewModel : PropertyChangedBase
    {
        private string _inBold;
        private string _explanation;

        public string InBold
        {
            get { return _inBold; }
            set
            {
                if (value == _inBold) return;
                _inBold = value;
                NotifyOfPropertyChange();
            }
        }

        public string Explanation
        {
            get { return _explanation; }
            set
            {
                if (value == _explanation) return;
                _explanation = value;
                NotifyOfPropertyChange();
            }
        }

    }
}
