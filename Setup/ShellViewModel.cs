using System.Collections.Generic;
using System.Windows;
using Caliburn.Micro;
using Microsoft.Win32;

namespace Setup
{
    public class ShellViewModel : Caliburn.Micro.Screen, IShell
    {
        private CurrentInstallation _currentInstallation;
        private IWindowManager _windowManager;
        public LicenseAgreementViewModel LicenseAgreementViewModel { get; set; }
        public InstallationFolderViewModel InstallationFolderViewModel { get; set; }
        public InstTypeChoiceViewModel InstTypeChoiceViewModel { get; set; }
        public ProcessProgressViewModel ProcessProgressViewModel { get; set; }

        private SetupPages _currentPage;

        public ShellViewModel(CurrentInstallation currentInstallation, IWindowManager windowManager,
            LicenseAgreementViewModel licenseAgreementViewModel,
            InstallationFolderViewModel installationFolderViewModel,
            InstTypeChoiceViewModel instTypeChoiceViewModel,
            ProcessProgressViewModel processProgressViewModel)
        {
            _currentInstallation = currentInstallation;
            _windowManager = windowManager;
            LicenseAgreementViewModel = licenseAgreementViewModel;
            InstallationFolderViewModel = installationFolderViewModel;
            InstTypeChoiceViewModel = instTypeChoiceViewModel;
            ProcessProgressViewModel = processProgressViewModel;

            _currentPage = SetupPages.Welcome;
            Do();
        }
        protected override void OnViewLoaded(object view)
        {
            DisplayName = $"{_currentInstallation.FullName} setup";
        }

        const string userRoot = "HKEY_LOCAL_MACHINE";
        const string RegistryBranch = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Iit\FiberTest20\";
        const string RegistryInstallerCultureKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Iit\FiberTest20\Culture\";

        private bool IsFibertestInstalled(out string culture)
        {
            culture = "";
            Dictionary<string, object> valuesBynames = new Dictionary<string, object>();
            using (RegistryKey rootKey = Registry.LocalMachine.OpenSubKey(RegistryBranch))
            {
                if (rootKey == null) return false;

//                culture = (string)Registry.GetValue(RegistryBranch2, "Culture", "none");
                culture = (string)Registry.GetValue(userRoot+"\\"+RegistryBranch, "Culture", "none");
               
                return true;
            }
        }

        private string GetInstallationCulture()
        {
            return (string)Registry.GetValue(userRoot + "\\" + RegistryBranch, "Culture", "none");
        }

        private void SaveSetupCultureInRegistry(string culture)
        {
            var result = Registry.LocalMachine.CreateSubKey(RegistryBranch);
            result.SetValue("Culture", culture);
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
                    var cu = GetInstallationCulture();
                    if (cu != null && cu != "none")
                    {
//                        var vm = new MyMessageBoxViewModel(MessageType.Confirmation, "Fibertest 2.0 installed on your PC already. Continue?");
//                        _windowManager.ShowDialog(vm);
//                        if (!vm.IsAnswerPositive)
                            TryClose();
                    }
                    else
                    {
                        SaveSetupCultureInRegistry("en-US");
                    }
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