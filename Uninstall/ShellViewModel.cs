using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Uninstall
{
    public class ShellViewModel : Screen, IShell
    {
        public string MainName = "IIT Fibertest 2.0";
        private bool _isFibertestUninstalled;
        public HeaderViewModel HeaderViewModel { get; set; }
        public UnInstallFolderViewModel UnInstallFolderViewModel { get; set; }
        public ProcessProgressViewModel ProcessProgressViewModel { get; set; }


        #region Buttons
        private bool _isButtonUninstallEnabled;
        public bool IsButtonUninstallEnabled
        {
            get => _isButtonUninstallEnabled;
            set
            {
                if (value == _isButtonUninstallEnabled) return;
                _isButtonUninstallEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        private bool _isButtonCancelEnabled;
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

        private string _lastButtonContent;
        public string LastButtonContent
        {
            get => _lastButtonContent;
            set
            {
                if (value == _lastButtonContent) return;
                _lastButtonContent = value;
                NotifyOfPropertyChange();
            }
        }

        private Brush _lastButtonBrush = Brushes.Black;
        public Brush LastButtonColor
        {
            get => _lastButtonBrush;
            set
            {
                if (value == _lastButtonBrush) return;
                _lastButtonBrush = value;
                NotifyOfPropertyChange();
            }
        }
        #endregion

        private readonly bool _isOnRtu;

        public ShellViewModel()
        {
            HeaderViewModel = new HeaderViewModel();
            HeaderViewModel.InBold = string.Format(Resources.SID_Uninstall__0_, MainName);
            HeaderViewModel.Explanation = string.Format(Resources.SID_Remove__0__from_your_computer, MainName);

            var iniFile = new IniFile();
            iniFile.AssignFile("uninstall.ini");
            _isOnRtu = iniFile.Read(IniSection.Uninstall, IniKey.IsOnRtu, false);

            UnInstallFolderViewModel =
                new UnInstallFolderViewModel(_isOnRtu ? Visibility.Collapsed : Visibility.Visible)
                { Visibility = Visibility.Visible };
            ProcessProgressViewModel = new ProcessProgressViewModel() { Visibility = Visibility.Collapsed };
            LastButtonContent = "Cancel";
            IsButtonUninstallEnabled = true;
            IsButtonCancelEnabled = true;

        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = string.Format(Resources.SID_Uninstall__0_, MainName);
        }

        public void Uninstall()
        {
            IsButtonUninstallEnabled = false;
            IsButtonCancelEnabled = false;
            UnInstallFolderViewModel.Visibility = Visibility.Collapsed;
            ProcessProgressViewModel.Visibility = Visibility.Visible;

            ProcessProgressViewModel.PropertyChanged += ProcessProgressViewModel_PropertyChanged;
            ProcessProgressViewModel.RunUninstall(
                UnInstallFolderViewModel.InstallationFolder, UnInstallFolderViewModel.IsFullUninstall, _isOnRtu);
        }

        private void ProcessProgressViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (ProcessProgressViewModel.IsUninstallSuccessful)
            {
                HeaderViewModel.Explanation = Resources.SID_Uninstallation_completed_successfully;
                LastButtonContent = Resources.SID_Done;
            }
            else
            {
                HeaderViewModel.Explanation = string.Format(Resources.SID_Failed_to_uninstall__0_, MainName);
                HeaderViewModel.FontColor = Brushes.Red;
                LastButtonContent = Resources.SID_Close;
                LastButtonColor = Brushes.Red;
            }
            IsButtonCancelEnabled = true;
            _isFibertestUninstalled = ProcessProgressViewModel.IsUninstallSuccessful;
        }

        public void LastButton()
        {
            if (_isFibertestUninstalled)
            {
                var folder = UnInstallFolderViewModel.IsFullUninstall
                    ? FileOperations.GetParentFolder(AppDomain.CurrentDomain.BaseDirectory)
                    : AppDomain.CurrentDomain.BaseDirectory;
                Console.WriteLine(folder);
                Process.Start("cmd.exe", $"/C ping 1.1.1.1 -n 3 -w 20 > Nul & RmDir /S /Q {folder}");
                Application.Current.Shutdown();
            }
            TryClose();
        }
    }
}