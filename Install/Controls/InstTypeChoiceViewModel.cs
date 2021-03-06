﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace Iit.Fibertest.Install
{
    public class InstTypeChoiceViewModel : PropertyChangedBase
    {
        private readonly CurrentInstallation _currentInstallation;

        private Visibility _visibility = Visibility.Collapsed;
        public Visibility Visibility
        {
            get => _visibility;
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

        private bool _isWebNeeded;
        public bool IsWebNeeded
        {
            get => _isWebNeeded;
            set
            {
                if (value == _isWebNeeded) return;
                if (value && !IsIisOk()) return;
                _isWebNeeded = value;
                WebSettingsVisibility = _isWebNeeded ? Visibility.Visible : Visibility.Collapsed;
                if (_isWebNeeded)
                    InitializeWebSettingsFromPreviousInstallation();
                NotifyOfPropertyChange();
            }
        }

        private Visibility _webSettingsVisibility = Visibility.Collapsed;
        public Visibility WebSettingsVisibility
        {
            get => _webSettingsVisibility;
            set
            {
                if (value == _webSettingsVisibility) return;
                _webSettingsVisibility = value;
                NotifyOfPropertyChange();
            }
        }

        private bool _isWebByHttps;
        public bool IsWebByHttps
        {
            get => _isWebByHttps;
            set
            {
                if (value == _isWebByHttps) return;
                _isWebByHttps = value;
                HttpsCertVisibility = _isWebByHttps ? Visibility.Visible : Visibility.Collapsed;
                NotifyOfPropertyChange();
            }
        }

        private Visibility _httpsCertVisibility = Visibility.Collapsed;
        public Visibility HttpsCertVisibility
        {
            get => _httpsCertVisibility;
            set
            {
                if (value == _httpsCertVisibility) return;
                _httpsCertVisibility = value;
                NotifyOfPropertyChange();
            }
        }

        private Visibility _dataCenterSettingsVisibility = Visibility.Collapsed;
        public Visibility DataCenterSettingsVisibility
        {
            get => _dataCenterSettingsVisibility;
            set
            {
                if (value == _dataCenterSettingsVisibility) return;
                _dataCenterSettingsVisibility = value;
                NotifyOfPropertyChange();
            }
        }

        public HeaderViewModel HeaderViewModel { get; set; } = new HeaderViewModel();
        public string Text1 { get; set; }
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
                DataCenterSettingsVisibility = _selectedType == "Data Center" ? Visibility.Visible : Visibility.Hidden;
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

        private string _filename;
        public string Filename
        {
            get => _filename;
            set
            {
                if (value == _filename) return;
                _filename = value;
                NotifyOfPropertyChange();
            }
        }

        private string _password;
        public string Password
        {
            get => _password;
            set
            {
                if (value == _password) return;
                _password = value;
                NotifyOfPropertyChange();
            }
        }

        private string _domainName;
        public string DomainName
        {
            get => _domainName;
            set
            {
                if (value == _domainName) return;
                _domainName = value;
                NotifyOfPropertyChange();
            }
        }

        public InstTypeChoiceViewModel(CurrentInstallation currentInstallation)
        {
            _currentInstallation = currentInstallation;
            HeaderViewModel.InBold = Resources.SID_Installation_Type;
            HeaderViewModel.Explanation = string.Format(Resources.SID_Please_select_the_type_of__0__install, currentInstallation.MainName);

            Text1 = string.Format(Resources.SID_Select_the_type_of__0__install__Click_Next_to_continue_, currentInstallation.MainName);
            InstTypes = new List<string>() { "Client", "Data Center", "Super Client" };
            SelectedType = InstTypes[0];
            Certificates = IisOperations.GetCertificates().ToList();
        }

        private void InitializeWebSettingsFromPreviousInstallation()
        {
            GetPreviousSettings();
            IsWebByHttps = _currentInstallation.IsWebByHttps;
            SelectedCertificate = string.IsNullOrEmpty(_currentInstallation.SslCertificateName)
                ? Certificates.FirstOrDefault()
                : _currentInstallation.SslCertificateName;
            DomainName = _currentInstallation.SslCertificateDomain;
            Filename = _currentInstallation.SslCertificatePath;
            Password = _currentInstallation.SslCertificatePassword;
        }

        private void GetPreviousSettings()
        {
            var service = FtServices.List.First(s => s.Name == "FibertestWaService");
            var settingsFilename = _currentInstallation.InstallationFolder + service.FolderInsideFibertest + @"/ini/settings.json";
            try
            {
                var json = File.ReadAllText(settingsFilename);
                WebApiSettings wcs = JsonConvert.DeserializeObject<WebApiSettings>(json);
                if (wcs == null) return;

                _currentInstallation.IsWebByHttps = wcs.ApiProtocol == "https";
                _currentInstallation.SslCertificateName = wcs.SslCertificateName;
                _currentInstallation.SslCertificateDomain = wcs.SslCertificateDomain;
                _currentInstallation.SslCertificatePath = wcs.SslCertificatePath;
                _currentInstallation.SslCertificatePassword = AesExt.Decrypt(wcs.SslCertificatePassword);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
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

        private bool IsIisOk()
        {
            var iisVersion = RegistryOperations.CheckIisVersion();
            if (iisVersion >= 10) return true;
            MessageBox.Show(
                iisVersion == -1
                    ? Resources.SID_Iis_not_found
                    : string.Format(Resources.SID_Iis_version_is, iisVersion),
                Resources.SID_Error_, MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        public void ChooseCertificateFile()
        {
            var fd = new OpenFileDialog();
            fd.Filter = @"SSL certificate files (*.pfx)|*.pfx";
            fd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            if (fd.ShowDialog() == true)
                Filename = fd.FileName;
        }
    }
}
