using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Iit.Fibertest.TestBench
{
    public class RadioButtonModel : INotifyPropertyChanged
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
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}