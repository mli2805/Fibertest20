using System.Collections.Generic;
using System.Linq;
using System.Windows;
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

        private string _mySqlTcpPort;
        public string MySqlTcpPort
        {
            get => _mySqlTcpPort;
            set
            {
                if (value == _mySqlTcpPort) return;
                _mySqlTcpPort = value;
                NotifyOfPropertyChange();
            }
        }

        private bool _isWebNeeded = true;
        public bool IsWebNeeded
        {
            get => _isWebNeeded;
            set
            {
                if (value == _isWebNeeded) return;
                _isWebNeeded = value;
                NotifyOfPropertyChange();
            }
        }

        private bool _isWebByHttps = true;
        public bool IsWebByHttps
        {
            get => _isWebByHttps;
            set
            {
                if (value == _isWebByHttps) return;
                _isWebByHttps = value;
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

        public HeaderViewModel HeaderViewModel { get; set; } = new HeaderViewModel();
        public string Text1 { get; set; }
        public string Text2 { get; set; } = Resources.SID_Type_of_install_;
        public List<string> InstTypes { get; set; }
        public List<string> Certificates { get; set; }

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

        private string _selectedCertificate;
        public string SelectedCertificate
        {
            get => _selectedCertificate;
            set
            {
                if (value == _selectedCertificate) return;
                _selectedCertificate = value;
                NotifyOfPropertyChange();
            }
        }

        public InstTypeChoiceViewModel(CurrentInstallation currentInstallation)
        {
            HeaderViewModel.InBold = Resources.SID_Installation_Type;
            HeaderViewModel.Explanation = string.Format(Resources.SID_Please_select_the_type_of__0__install, currentInstallation.MainName);

            Text1 = string.Format(Resources.SID_Select_the_type_of__0__install__Click_Next_to_continue_, currentInstallation.MainName);
            InstTypes = new List<string>() { "Data Center", "Client", "Super Client" };
            SelectedType = InstTypes[0];

            Certificates = IisOperations.GetCertificates().ToList();
            SelectedCertificate = Certificates.FirstOrDefault();
        }

        public InstallationType GetSelectedType()
        {
            switch (SelectedType)
            {
                case "Data Center": return InstallationType.Datacenter;
                case "Client": return InstallationType.Client;
                case "Super Client": return InstallationType.SuperClient;
                default: return InstallationType.Client;
            }

        }
    }
}
