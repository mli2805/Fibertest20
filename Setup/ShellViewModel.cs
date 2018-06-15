using System.Windows;

namespace Setup
{
    public class ShellViewModel : Caliburn.Micro.Screen, IShell
    {
        private CurrentInstallation _currentInstallation;
        public LicenseAgreementViewModel LicenseAgreementViewModel { get; set; } = new LicenseAgreementViewModel();
        public InstallationFolderViewModel InstallationFolderViewModel { get; set; }
        public InstTypeChoiceViewModel InstTypeChoiceViewModel { get; set; } = new InstTypeChoiceViewModel();
        public ProcessProgressViewModel ProcessProgressViewModel { get; set; } = new ProcessProgressViewModel();

        private SetupPages _currentPage;

        public ShellViewModel(CurrentInstallation currentInstallation, InstallationFolderViewModel installationFolderViewModel)
        {
            _currentInstallation = currentInstallation;
            InstallationFolderViewModel = installationFolderViewModel;

            _currentPage = SetupPages.Welcome;
            Do();
        }
        protected override void OnViewLoaded(object view)
        {
            DisplayName = $"{_currentInstallation.FullName} setup";
        }

        public void Next()
        {
            _currentPage++;
            Do();
        }

        public void Back()
        {
            _currentPage--;
            Do();
        }

        private void Do()
        {
            switch (_currentPage)
            {
                case SetupPages.Welcome:
                    LicenseAgreementViewModel.Visibility = Visibility.Collapsed;
                    InstallationFolderViewModel.Visibility = Visibility.Collapsed;
                    InstTypeChoiceViewModel.Visibility = Visibility.Collapsed;
                    ProcessProgressViewModel.Visibility = Visibility.Collapsed;
                    break;
                case SetupPages.LicenseAgreement:
                    LicenseAgreementViewModel.Visibility = Visibility.Visible;
                    InstallationFolderViewModel.Visibility = Visibility.Collapsed;
                    InstTypeChoiceViewModel.Visibility = Visibility.Collapsed;
                    ProcessProgressViewModel.Visibility = Visibility.Collapsed;
                    break;
                case SetupPages.InstallationFolder:
                    LicenseAgreementViewModel.Visibility = Visibility.Collapsed;
                    InstallationFolderViewModel.Visibility = Visibility.Visible;
                    InstTypeChoiceViewModel.Visibility = Visibility.Collapsed;
                    break;
                case SetupPages.InstTypeChoice:
                    LicenseAgreementViewModel.Visibility = Visibility.Collapsed;
                    InstallationFolderViewModel.Visibility = Visibility.Collapsed;
                    InstTypeChoiceViewModel.Visibility = Visibility.Visible;
                    ProcessProgressViewModel.Visibility = Visibility.Collapsed;
                    break;
                case SetupPages.ProcessProgress:
                    LicenseAgreementViewModel.Visibility = Visibility.Collapsed;
                    InstallationFolderViewModel.Visibility = Visibility.Collapsed;
                    InstTypeChoiceViewModel.Visibility = Visibility.Collapsed;
                    ProcessProgressViewModel.Visibility = Visibility.Visible;
                    break;
            }
        }

        public void Cancel()
        {
            TryClose();
        }
    }
}