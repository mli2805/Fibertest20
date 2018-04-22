using Caliburn.Micro;

namespace Iit.Fibertest.Client
{
    public class BoolWithNotification : PropertyChangedBase
    {
        private bool _isChecked;
        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                _isChecked = value;
                NotifyOfPropertyChange();
            }
        }
    }
}