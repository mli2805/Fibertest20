using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.Uninstall;

namespace Uninstall
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
        #endregion

        public ShellViewModel()
        {
            HeaderViewModel = new HeaderViewModel();
            HeaderViewModel.InBold = $"Uninstall {MainName}";
            HeaderViewModel.Explanation = $"Remove {MainName} from your computer";

            UnInstallFolderViewModel = new UnInstallFolderViewModel() { Visibility = Visibility.Visible };
            ProcessProgressViewModel = new ProcessProgressViewModel() { Visibility = Visibility.Collapsed };
            LastButtonContent = "Cancel";
            IsButtonUninstallEnabled = true;
            IsButtonCancelEnabled = true;
        }

        public void Uninstall()
        {
            IsButtonUninstallEnabled = false;
            IsButtonCancelEnabled = false;
            UnInstallFolderViewModel.Visibility = Visibility.Collapsed;
            ProcessProgressViewModel.Visibility = Visibility.Visible;

            ProcessProgressViewModel.RunUninstall(
                UnInstallFolderViewModel.InstallationFolder, UnInstallFolderViewModel.IsFullUninstall);

            LastButtonContent = "Close";
            _isFibertestUninstalled = true;
            IsButtonCancelEnabled = true;
        }

        public void LastButton()
        {
            if (_isFibertestUninstalled)
            {
                Process.Start("cmd.exe", "/C ping 1.1.1.1 -n 3 -w 20 & RmDir /S /Q " +
                                         AppDomain.CurrentDomain.BaseDirectory + "");
                Application.Current.Shutdown();
            }
            TryClose();
        }
    }
}