using Caliburn.Micro;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class NoLicenseAppliedViewModel : Screen
    {
        private readonly LicenseSender _licenseSender;

        public bool IsCommandSent { get; set; }
        public bool IsLicenseAppliedSuccessfully { get; set; }

        public NoLicenseAppliedViewModel(LicenseSender licenseSender)
        {
            _licenseSender = licenseSender;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Attention_;
        }

        public async void ApplyDemoLicense()
        {
            IsCommandSent = true;
            IsLicenseAppliedSuccessfully = await _licenseSender.ApplyDemoLicense();
            TryClose();
        }

        public async void LoadLicenseFromFile()
        {
            IsCommandSent = true;
            IsLicenseAppliedSuccessfully = await _licenseSender.ApplyLicenseFromFile();
            TryClose();
        }
    }
}
