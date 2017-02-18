using Caliburn.Micro;

namespace Iit.Fibertest.TestBench
{
    public class RadioButtonModel : PropertyChangedBase
    {
        public string Title { get; set; }
        public bool IsEnabled { get; set; } = true;

        private bool _isChecked;
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (value == _isChecked) return;
                _isChecked = value;
                NotifyOfPropertyChange();
            }
        }

    }
}