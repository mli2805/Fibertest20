using System.Collections.ObjectModel;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Setup
{
    public class ProcessProgressViewModel : PropertyChangedBase
    {
        private readonly CurrentInstallation _currentInstallation;
        private readonly SetupManager _setupManager;
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

        public ObservableCollection<string> ProgressLines { get; set; } = new ObservableCollection<string>();

        public string Text1 { get; set; }

        public ProcessProgressViewModel(CurrentInstallation currentInstallation, SetupManager setupManager)
        {
            _currentInstallation = currentInstallation;
            _setupManager = setupManager;
            HeaderViewModel.InBold = Resources.SID_Installing;
            HeaderViewModel.Explanation = string.Format(Resources.SID_Please_wait_while__0__is_being_installed,
                _currentInstallation.MainName);
        }

        public void RunSetup()
        {
            if (_setupManager.Run(ProgressLines))
                SaySuccess();
            else SayFail();
        }

        private void SaySuccess()
        {
            HeaderViewModel.InBold = Resources.SID_Installation_Complete;
            HeaderViewModel.Explanation = Resources.SID_Setup_was_completed_successfully;
        }

      
        private void SayFail()
        {
            HeaderViewModel.InBold = "Installation failed";
            HeaderViewModel.Explanation = $"{_currentInstallation.MainName} installation failed.";
        }
    }
}