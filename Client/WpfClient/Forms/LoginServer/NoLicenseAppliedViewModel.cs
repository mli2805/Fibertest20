﻿using Caliburn.Micro;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class NoLicenseAppliedViewModel : Screen
    {
        public readonly LicenseSender LicenseSender;

        public bool IsCommandSent { get; set; }
        public bool IsLicenseAppliedSuccessfully { get; set; }
        public string SecurityAdminPassword;

        public NoLicenseAppliedViewModel(LicenseSender licenseSender)
        {
            LicenseSender = licenseSender;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Attention_;
        }

        public async void ApplyDemoLicense()
        {
            IsCommandSent = true;
            IsLicenseAppliedSuccessfully = await LicenseSender.ApplyDemoLicense();
            TryClose();
        }

        // It is a button handler, but when called from tests receives parameter
        public async void LoadLicenseFromFile(string licenseFilename)
        {
            IsCommandSent = true;
            IsLicenseAppliedSuccessfully = await LicenseSender.ApplyLicenseFromFile(licenseFilename);
            SecurityAdminPassword = LicenseSender.SecurityAdminPassword;
            TryClose();
        }
    }
}
