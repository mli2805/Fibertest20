using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class LicenseViewModel : Screen
    {
        private readonly Model _readModel;
        private readonly LicenseSender _licenseSender;
        private License _selectedLicense;
        public LicenseControlViewModel LicenseControlViewModel { get; set; } = new LicenseControlViewModel();

        public List<License> Licenses { get; set; }

        public License SelectedLicense
        {
            get => _selectedLicense;
            set
            {
                if (Equals(value, _selectedLicense)) return;
                _selectedLicense = value;
                NotifyOfPropertyChange();
                LicenseControlViewModel.License = SelectedLicense;
            }
        }

        public bool IsListVisible => _readModel.Licenses.Count > 1;

        public bool IsApplyLicenseEnabled { get; set; }


        public LicenseViewModel(Model readModel, CurrentUser currentUser, LicenseSender licenseSender)
        {
            _readModel = readModel;
            _licenseSender = licenseSender;
            IsApplyLicenseEnabled = currentUser.Role <= Role.Root;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_License;
        }

        public void Initialize()
        {
            Licenses = _readModel.Licenses;
            SelectedLicense = Licenses.First();
            LicenseControlViewModel.License = SelectedLicense;
        }

        public async void ApplyLicFile()
        {
            await _licenseSender.ApplyLicenseFromFile();
        }
    }
}
