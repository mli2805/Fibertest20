using System;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public class LicenseViewModel : Screen
    {
        private readonly Model _readModel;
        private readonly LicenseManager _licenseManager;
        private readonly IWcfServiceDesktopC2D _c2DWcfManager;
        private readonly IWindowManager _windowManager;
        public LicenseControlViewModel LicenseControlViewModel { get; set; } = new LicenseControlViewModel();
        private int _selectedLicenseIndex;

        private Visibility _previousButtonVisibility;
        public Visibility PreviousButtonVisibility
        {
            get => _previousButtonVisibility;
            set
            {
                if (value == _previousButtonVisibility) return;
                _previousButtonVisibility = value;
                NotifyOfPropertyChange();
            }
        }

        private Visibility _nextButtonVisibility;
        public Visibility NextButtonVisibility
        {
            get => _nextButtonVisibility;
            set
            {
                if (value == _nextButtonVisibility) return;
                _nextButtonVisibility = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsApplyLicenseEnabled { get; set; }

        public LicenseViewModel(Model readModel, LicenseManager licenseManager, 
            IWcfServiceDesktopC2D c2DWcfManager, IWindowManager windowManager, CurrentUser currentUser)
        {
            _readModel = readModel;
            _licenseManager = licenseManager;
            _c2DWcfManager = c2DWcfManager;
            _windowManager = windowManager;
             IsApplyLicenseEnabled = currentUser.Role <= Role.Root;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_License;
        }

        public void Initialize()
        {
            LicenseControlViewModel.License = _readModel.Licenses[0];
            _selectedLicenseIndex = 0;
            PreviousButtonVisibility = Visibility.Hidden;
            NextButtonVisibility = _readModel.Licenses.Count > 1 ? Visibility.Visible : Visibility.Hidden;
        }

        public void ShowPreviousLicense()
        {
            if (_selectedLicenseIndex > 0)
                LicenseControlViewModel.License = _readModel.Licenses[--_selectedLicenseIndex];
            NextButtonVisibility = Visibility.Visible;
            if (_selectedLicenseIndex == 0)
                PreviousButtonVisibility = Visibility.Hidden;
        }

        public void ShowNextLicense()
        {
            if (_selectedLicenseIndex < _readModel.Licenses.Count - 1)
                LicenseControlViewModel.License = _readModel.Licenses[++_selectedLicenseIndex];
            PreviousButtonVisibility = Visibility.Visible;
            if (_selectedLicenseIndex == _readModel.Licenses.Count - 1)
                NextButtonVisibility = Visibility.Hidden;
        }

        public async void ApplyLicFile()
        {
            var licenseInFile = _licenseManager.ReadLicenseFromFileDialog();
            if (licenseInFile == null)
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Invalid_license_file_);
                _windowManager.ShowDialogWithAssignedOwner(vm);
                return;
            }

            var cmd = new ApplyLicense()
            {
                LicenseId = licenseInFile.LicenseId,
                IsIncremental = licenseInFile.IsIncremental,
                Owner = licenseInFile.Owner,
                RtuCount = new LicenseParameter(licenseInFile.RtuCount),
                ClientStationCount = new LicenseParameter(licenseInFile.ClientStationCount),
                WebClientCount = new LicenseParameter(licenseInFile.WebClientCount),
                SuperClientStationCount = new LicenseParameter(licenseInFile.SuperClientStationCount),
                CreationDate = licenseInFile.CreationDate,
                LoadingDate = DateTime.Today,
                Version = licenseInFile.Version,
            };
            var result = await _c2DWcfManager.SendCommandAsObj(cmd);
            if (result != null)
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, @"ApplyLicense: " + result);
                _windowManager.ShowDialogWithAssignedOwner(vm);
            }
            else
            {
                var vm = new MyMessageBoxViewModel(MessageType.Information, Resources.SID_License_applied_successfully_);
                _windowManager.ShowDialogWithAssignedOwner(vm);
                TryClose();
            }
        }
    }
}
