using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;
using Caliburn.Micro;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Install
{
    public class InstTypeChoiceViewModel : PropertyChangedBase
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

        public string MySqlTcpPort { get; set; }

        private bool _isWebNeeded = true;
        public bool IsWebNeeded
        {
            get => _isWebNeeded;
            set
            {
                if (value == _isWebNeeded) return;
                _isWebNeeded = value;
                NotifyOfPropertyChange();
                ArchiveErrorMessageVisibility = _isWebNeeded && !_webArchivePath.EndsWith(".zip")
                    ? Visibility.Visible
                    : Visibility.Hidden;
            }
        }

        private string _webArchivePath = @"c:\";
        public string WebArchivePath
        {
            get => _webArchivePath;
            set
            {
                if (value == _webArchivePath) return;
                _webArchivePath = value;
                NotifyOfPropertyChange();
            }
        }

        private Visibility _mySqlTcpPortVisibility = Visibility.Collapsed;
        public Visibility MySqlTcpPortVisibility
        {
            get { return _mySqlTcpPortVisibility; }
            set
            {
                if (value == _mySqlTcpPortVisibility) return;
                _mySqlTcpPortVisibility = value;
                NotifyOfPropertyChange();
            }
        }

        private Visibility _archiveErrorMessageVisibility = Visibility.Hidden;
        public Visibility ArchiveErrorMessageVisibility
        {
            get => _archiveErrorMessageVisibility;
            set
            {
                if (value == _archiveErrorMessageVisibility) return;
                _archiveErrorMessageVisibility = value;
                NotifyOfPropertyChange();
            }
        }

        public HeaderViewModel HeaderViewModel { get; set; } = new HeaderViewModel();
        public string Text1 { get; set; }
        public string Text2 { get; set; } = Resources.SID_Type_of_install_;
        public List<string> InstTypes { get; set; }

        private string _selectedType;
        public string SelectedType
        {
            get => _selectedType;
            set
            {
                if (value == _selectedType) return;
                _selectedType = value;
                MySqlTcpPortVisibility = _selectedType == "Data Center" ? Visibility.Visible : Visibility.Hidden;
                NotifyOfPropertyChange();
            }
        }

        public InstTypeChoiceViewModel(CurrentInstallation currentInstallation)
        {
            HeaderViewModel.InBold = Resources.SID_Installation_Type;
            HeaderViewModel.Explanation = string.Format(Resources.SID_Please_select_the_type_of__0__install, currentInstallation.MainName);

            Text1 = string.Format(Resources.SID_Select_the_type_of__0__install__Click_Next_to_continue_, currentInstallation.MainName);
            InstTypes = new List<string>() { "RTU Manager", "Client", "Data Center", "Super Client" };
            SelectedType = InstTypes[0];

            var port = IniOperations.GetMysqlTcpPort(currentInstallation.InstallationFolder);
            MySqlTcpPort = port != "error" ? port : "3306";
        }

        public InstallationType GetSelectedType()
        {
            switch (SelectedType)
            {
                case "Client": return InstallationType.Client;
                case "Data Center": return InstallationType.Datacenter;
                case "RTU Manager": return InstallationType.RtuManager;
                case "Super Client": return InstallationType.SuperClient;
                default: return InstallationType.Client;
            }

        }

        public void Browse()
        {
            string initialDirectory = string.IsNullOrEmpty(WebArchivePath)
                ? Environment.GetFolderPath(Environment.SpecialFolder.Personal)
                : WebArchivePath;
            using (var dialog = new OpenFileDialog(){InitialDirectory = initialDirectory, Filter = @"archive|*.zip"})
            {
                var result = dialog.ShowDialog();
                if (result != DialogResult.OK) return;

                WebArchivePath = dialog.FileName;
            }

            if (!WebArchivePath.EndsWith(".zip"))
                ArchiveErrorMessageVisibility = Visibility.Visible;
        }
    }
}
