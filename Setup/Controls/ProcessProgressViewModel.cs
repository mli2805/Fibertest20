using System.Windows;
using Caliburn.Micro;

namespace Setup
{
    public class ProcessProgressViewModel : PropertyChangedBase
    {
        private Visibility _visibility = Visibility.Collapsed;

        public Visibility Visibility
        {
            get { return _visibility; }
            set
            {
                if (value == _visibility) return;
                _visibility = value;
                NotifyOfPropertyChange();
            }
        }

        public HeaderViewModel HeaderViewModel { get; set; } = new HeaderViewModel();

        public ProcessProgressViewModel()
        {
            HeaderViewModel.InBold = "Installing";
            HeaderViewModel.Explanation = "Please wait while IIT Fibertest 2.0 is being installed.";
        }

        public void SayGoodbye()
        {
            HeaderViewModel.InBold = "Installation Complete";
            HeaderViewModel.Explanation = "Setup was completed successfully";
        }
    }
}
