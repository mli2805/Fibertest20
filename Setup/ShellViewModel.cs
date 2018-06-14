namespace Setup
{
    public class ShellViewModel : Caliburn.Micro.Screen, IShell
    {
        public LicenseAgreementViewModel LicenseAgreementViewModel { get; set; } = new LicenseAgreementViewModel();
        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Fibertest 2.0 setup";
        }
    }
}