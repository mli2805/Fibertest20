using System.Windows;
using Caliburn.Micro;

namespace Setup
{
    public class FarewellViewModel : PropertyChangedBase
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

        public FarewellViewModel()
        {
            HeaderViewModel.InBold = "Installation Complete";
            HeaderViewModel.Explanation = "Setup was completed successfully";
        }
    }
}
