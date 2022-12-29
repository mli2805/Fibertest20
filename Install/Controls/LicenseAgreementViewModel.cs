using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using Caliburn.Micro;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Install
{
    public class LicenseAgreementViewModel : Screen
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

        public HeaderViewModel HeaderViewModel { get; set; } = new HeaderViewModel();
        public string Text1 { get; set; }
        public FixedDocumentSequence FixedDocumentSequence { get; set; }

        public List<string> License { get; set; } = new List<string>()
        {
            Resources.SID_Institute_of_Information_Technologies,
            Resources.SID_Iit_address,
            Resources.SID_Iit_Phone,
            "Web:    www.beliit.com",
            "E-Mail: info@beliit.com",
            "",

        };
        public List<string> License2 { get; set; } = new List<string>()
        {
            "Copyright © 2003-2023, "+ Resources.SID_Institute_of_Information_Technologies,
            Resources.SID_All_rights_reserved,
        };

        public LicenseAgreementViewModel(CurrentInstallation currentInstallation)
        {
            HeaderViewModel.InBold = Resources.SID_License_Agreement;
            HeaderViewModel.Explanation = string.Format(Resources.SID_Please_review_the_license_terms_before_installing__0__, currentInstallation.MainName);
            Text1 = string.Format(Resources.SID_If_you_accept_0_, currentInstallation.MainName);
        }
    }
}
