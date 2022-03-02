using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Install
{
    public class ShellViewModel : Screen, IShell
    {
        private IMyLog _logFile;
        private CurrentInstallation _currentInstallation;
        public LicenseAgreementViewModel LicenseAgreementViewModel { get; set; }
        public InstallationFolderViewModel InstallationFolderViewModel { get; set; }
        public InstTypeChoiceViewModel InstTypeChoiceViewModel { get; set; }
        public ProcessProgressViewModel ProcessProgressViewModel { get; set; }

        private SetupPages _currentPage;

        #region Buttons
        private string _buttonNextContent;
        private string _buttonCancelContent;
        private string _buttonBackContent;
        private Brush _buttonNextColor = Brushes.Black;
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

        public Brush ButtonNextColor
        {
            get => _buttonNextColor;
            set
            {
                if (value == _buttonNextColor) return;
                _buttonNextColor = value;
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

        public ShellViewModel(CurrentInstallation currentInstallation, IMyLog logFile,
            LicenseAgreementViewModel licenseAgreementViewModel,
            InstallationFolderViewModel installationFolderViewModel,
            InstTypeChoiceViewModel instTypeChoiceViewModel,
            ProcessProgressViewModel processProgressViewModel)
        {
            _logFile = logFile;
            _currentInstallation = currentInstallation;
            LicenseAgreementViewModel = licenseAgreementViewModel;
            InstallationFolderViewModel = installationFolderViewModel;
            InstTypeChoiceViewModel = instTypeChoiceViewModel;
            ProcessProgressViewModel = processProgressViewModel;

            _currentPage = SetupPages.LicenseAgreement;
        }
        protected override void OnViewLoaded(object view)
        {
            var assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo info = FileVersionInfo.GetVersionInfo(assembly.Location);
            _currentInstallation.ProductVersion = info.FileVersion;

            DisplayName = string.Format(Resources.SID_Setup_caption, info.FileVersion);
            _logFile.AssignFile(@"Setup.log");
            _logFile.AppendLine(@"Setup application started!");
            Do();
        }


        public void Next()
        {
            _currentPage++;
            if (_currentPage > SetupPages.ProcessProgress)
            {
                TryClose();
            }
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

                    RegistryOperations.SaveFibertestValue("InstallationFolder", InstallationFolderViewModel.InstallationFolder);
                    _currentInstallation.InstallationFolder = InstallationFolderViewModel.InstallationFolder;

                    var port = IniOperations.GetMysqlTcpPort(_currentInstallation.InstallationFolder);
                    InstTypeChoiceViewModel.InstSettingsForServerViewModel.MySqlTcpPort = port != "error" ? port : "3306";

                    var isHigh = IniOperations.GetIsHighDensityGraph(_currentInstallation.InstallationFolder);
                    InstTypeChoiceViewModel.InstSettingsForClientViewModel.IsHighDensityGraph = isHigh;

                    ButtonBackContent = Resources.SID_Back;
                    IsButtonBackEnabled = true;
                    ButtonNextContent = Resources.SID_Next;
                    IsButtonNextEnabled = true;
                    ButtonCancelContent = Resources.SID_Cancel;
                    IsButtonCancelEnabled = true;

                    break;
                case SetupPages.ProcessProgress:
                    OpenProgressView();
                    ProcessProgressViewModel.PropertyChanged += ProcessProgressViewModel_PropertyChanged;
                    ProcessProgressViewModel.RunSetup();
                    break;
            }
        }

        private void OpenProgressView()
        {
            LicenseAgreementViewModel.Visibility = Visibility.Collapsed;
            InstallationFolderViewModel.Visibility = Visibility.Collapsed;
            InstTypeChoiceViewModel.Visibility = Visibility.Collapsed;
            ProcessProgressViewModel.Visibility = Visibility.Visible;

            ButtonBackContent = Resources.SID_Back;
            IsButtonBackEnabled = false;
            ButtonNextContent = Resources.SID_Wait___;
            IsButtonNextEnabled = false;
            ButtonCancelContent = Resources.SID_Cancel;
            IsButtonCancelEnabled = false;

            _currentInstallation.InstallationType = InstTypeChoiceViewModel.GetSelectedType();

            _currentInstallation.IsHighDensityGraph =
                InstTypeChoiceViewModel.InstSettingsForClientViewModel.IsHighDensityGraph;

            _currentInstallation.MySqlTcpPort = InstTypeChoiceViewModel.InstSettingsForServerViewModel.MySqlTcpPort;
            _currentInstallation.IsWebNeeded = InstTypeChoiceViewModel.InstSettingsForServerViewModel.IsWebNeeded;
            _currentInstallation.IsWebByHttps = InstTypeChoiceViewModel.InstSettingsForServerViewModel.IsWebByHttps;
            _currentInstallation.SslCertificateName = InstTypeChoiceViewModel.InstSettingsForServerViewModel.SelectedCertificate;
            _currentInstallation.SslCertificateDomain = InstTypeChoiceViewModel.InstSettingsForServerViewModel.DomainName;
            _currentInstallation.SslCertificatePath = InstTypeChoiceViewModel.InstSettingsForServerViewModel.Filename;
            _currentInstallation.SslCertificatePassword = AesExt.Encript(InstTypeChoiceViewModel.InstSettingsForServerViewModel.Password);
        }

        private void ProcessProgressViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsDone")
            {
                ButtonBackContent = Resources.SID_Back;
                IsButtonBackEnabled = false;
                if (ProcessProgressViewModel.SetupResult)
                    ButtonNextContent = Resources.SID_Done;
                else
                {
                    ButtonNextContent =  Resources.SID_Close;
                    ButtonNextColor = Brushes.Red;
                }
                IsButtonNextEnabled = true;
                ButtonCancelContent = Resources.SID_Cancel;
                IsButtonCancelEnabled = false;
            }
        }

        public void Cancel()
        {
            var result = MessageBox.Show(string.Format(Resources.SID_Are_you_sure_you_want_to_quit__0__setup_, 
                    _currentInstallation.MainName), Resources.SID_Confirmation, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
                TryClose();
        }
    }
}