using System.Threading;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.StringResources;

namespace Setup
{
    public class ShellViewModel : Screen, IShell
    {
        private CurrentInstallation _currentInstallation;
        private IWindowManager _windowManager;
        public LicenseAgreementViewModel LicenseAgreementViewModel { get; set; }
        public InstallationFolderViewModel InstallationFolderViewModel { get; set; }
        public InstTypeChoiceViewModel InstTypeChoiceViewModel { get; set; }
        public ProcessProgressViewModel ProcessProgressViewModel { get; set; }

        private SetupPages _currentPage;

        #region Buttons
        private string _buttonNextContent;
        private string _buttonCancelContent;
        private string _buttonBackContent;
        private bool _isButtonBackEnabled;
        private bool _isButtonNextEnabled;
        private bool _isButtonCancelEnabled;

        public string ButtonBackContent
        {
            get => _buttonBackContent;
            set
            {
                if (value == _buttonBackContent) return;
                _buttonBackContent = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsButtonBackEnabled
        {
            get => _isButtonBackEnabled;
            set
            {
                if (value == _isButtonBackEnabled) return;
                _isButtonBackEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        public string ButtonNextContent
        {
            get => _buttonNextContent;
            set
            {
                if (value == _buttonNextContent) return;
                _buttonNextContent = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsButtonNextEnabled
        {
            get => _isButtonNextEnabled;
            set
            {
                if (value == _isButtonNextEnabled) return;
                _isButtonNextEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        public string ButtonCancelContent
        {
            get => _buttonCancelContent;
            set
            {
                if (value == _buttonCancelContent) return;
                _buttonCancelContent = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsButtonCancelEnabled
        {
            get => _isButtonCancelEnabled;
            set
            {
                if (value == _isButtonCancelEnabled) return;
                _isButtonCancelEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        #endregion

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

            _currentPage = SetupPages.LicenseAgreement;
        }
        protected override void OnViewLoaded(object view)
        {
            DisplayName = string.Format(Resources.SID_Setup_caption, _currentInstallation.FullName);
            Do();
        }


        public void Next()
        {
            _currentPage++;
            if (_currentPage > SetupPages.ProcessProgress)
                TryClose();
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
                case SetupPages.LicenseAgreement:
                    LicenseAgreementViewModel.Visibility = Visibility.Visible;
                    InstallationFolderViewModel.Visibility = Visibility.Collapsed;
                    InstTypeChoiceViewModel.Visibility = Visibility.Collapsed;
                    ProcessProgressViewModel.Visibility = Visibility.Collapsed;

                    ButtonBackContent = Resources.SID_Back;
                    IsButtonBackEnabled = false;
                    ButtonNextContent = Resources.SID_I_Agree;
                    IsButtonNextEnabled = true;
                    ButtonCancelContent = Resources.SID_Cancel;
                    IsButtonCancelEnabled = true;

                    break;

                case SetupPages.InstallationFolder:
                    LicenseAgreementViewModel.Visibility = Visibility.Collapsed;
                    InstallationFolderViewModel.Visibility = Visibility.Visible;
                    InstTypeChoiceViewModel.Visibility = Visibility.Collapsed;

                    ButtonBackContent = Resources.SID_Back;
                    IsButtonBackEnabled = true;
                    ButtonNextContent = Resources.SID_Next;
                    IsButtonNextEnabled = true;
                    ButtonCancelContent = Resources.SID_Cancel;
                    IsButtonCancelEnabled = true;

                    break;
                case SetupPages.InstTypeChoice:
                    LicenseAgreementViewModel.Visibility = Visibility.Collapsed;
                    InstallationFolderViewModel.Visibility = Visibility.Collapsed;
                    InstTypeChoiceViewModel.Visibility = Visibility.Visible;
                    ProcessProgressViewModel.Visibility = Visibility.Collapsed;

                    ButtonBackContent = Resources.SID_Back;
                    IsButtonBackEnabled = true;
                    ButtonNextContent = Resources.SID_Next;
                    IsButtonNextEnabled = true;
                    ButtonCancelContent = Resources.SID_Cancel;
                    IsButtonCancelEnabled = true;

                    break;
                case SetupPages.ProcessProgress:
                    LicenseAgreementViewModel.Visibility = Visibility.Collapsed;
                    InstallationFolderViewModel.Visibility = Visibility.Collapsed;
                    InstTypeChoiceViewModel.Visibility = Visibility.Collapsed;
                    ProcessProgressViewModel.Visibility = Visibility.Visible;

                    ButtonBackContent = Resources.SID_Back;
                    IsButtonBackEnabled = false;
                    ButtonNextContent = Resources.SID_Next;
                    IsButtonNextEnabled = false;
                    ButtonCancelContent = Resources.SID_Cancel;
                    IsButtonCancelEnabled = false;

                    ProcessProgressViewModel.RunSetup();

                    ProcessProgressViewModel.SayGoodbye();

                    ButtonBackContent = Resources.SID_Back;
                    IsButtonBackEnabled = false;
                    ButtonNextContent = Resources.SID_Done;
                    IsButtonNextEnabled = true;
                    ButtonCancelContent = Resources.SID_Cancel;
                    IsButtonCancelEnabled = false;

                    break;
            }
        }

      
        public void Cancel()
        {
            var result = MessageBox.Show(string.Format(Resources.SID_Are_you_sure_you_want_to_quit__0__setup_, _currentInstallation.MainName), Resources.SID_Confirmation, MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
                TryClose();
        }
    }
}