using System.Windows;
using Caliburn.Micro;

namespace Setup
{
    public class InstallationFolderViewModel : PropertyChangedBase
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

        public InstallationFolderViewModel()
        {
            HeaderViewModel.InBold = "Choose Install Location";
            HeaderViewModel.Explanation = "Choose folder in which to install IIT Fibertest 2.0";
        }
    }
}
