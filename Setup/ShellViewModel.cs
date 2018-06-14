using System.Windows;

namespace Setup
{
    public class ShellViewModel : Caliburn.Micro.Screen, IShell
    {
        public LicenseAgreementViewModel LicenseAgreementViewModel { get; set; } = new LicenseAgreementViewModel();
        public InstallationFolderViewModel InstallationFolderViewModel { get; set; } = new InstallationFolderViewModel();
        public InstTypeChoiceViewModel InstTypeChoiceViewModel { get; set; } = new InstTypeChoiceViewModel();

        private SetupPages _currentPage;

        public ShellViewModel()
        {
            _currentPage = SetupPages.Welcome;
            ShowPage();
        }
        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Fibertest 2.0 setup";
        }

        public void Next()
        {
            _currentPage++;
            ShowPage();
        }

        public void Back()
        {
            _currentPage--;
            ShowPage();
        }

        private void ShowPage()
        {
            switch (_currentPage)
            {
                case SetupPages.Welcome:
                    LicenseAgreementViewModel.Visibility = Visibility.Collapsed;
                    InstallationFolderViewModel.Visibility = Visibility.Collapsed;
                    InstTypeChoiceViewModel.Visibility = Visibility.Collapsed;
                    break;
                case SetupPages.LicenseAgreement:
                    LicenseAgreementViewModel.Visibility = Visibility.Visible;
                    InstallationFolderViewModel.Visibility = Visibility.Collapsed;
                    InstTypeChoiceViewModel.Visibility = Visibility.Collapsed;
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
                    break;
                case SetupPages.ProgressPage:
                    LicenseAgreementViewModel.Visibility = Visibility.Collapsed;
                    InstallationFolderViewModel.Visibility = Visibility.Collapsed;
                    InstTypeChoiceViewModel.Visibility = Visibility.Collapsed;
                    break;
                case SetupPages.Farewell:
                    LicenseAgreementViewModel.Visibility = Visibility.Collapsed;
                    InstallationFolderViewModel.Visibility = Visibility.Collapsed;
                    InstTypeChoiceViewModel.Visibility = Visibility.Collapsed;
                    break;
                    
            }
        }

        public void Cancel()
        {
            TryClose();
        }
    }
}