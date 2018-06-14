using Caliburn.Micro;
using Iit.Fibertest.StringResources;

namespace Setup
{
    public class LicenseAgreementViewModel : PropertyChangedBase
    {
        public HeaderViewModel HeaderViewModel { get; set; } = new HeaderViewModel();
        public string LicenseRtf { get; set; }

        public LicenseAgreementViewModel()
        {
            HeaderViewModel.InBold = "Choose Install Location";
            HeaderViewModel.Explanation = "Choose folder in which to install IIT Fibertest 2.0";

            LicenseRtf = Resources.SID_license_en_rtf;
        }
    }
}
