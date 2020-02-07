using System;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Xps.Packaging;
using Caliburn.Micro;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.InstallRtu
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

        public LicenseAgreementViewModel(CurrentInstallation currentInstallation)
        {
            HeaderViewModel.InBold = Resources.SID_License_Agreement;
            HeaderViewModel.Explanation = string.Format(Resources.SID_Please_review_the_license_terms_before_installing__0__, currentInstallation.MainName);
            Text1 = string.Format(Resources.SID_If_you_accept_0_, currentInstallation.MainName);
            ReadLicense();
        }

        private void ReadLicense()
        {
            var filename = Resources.SID_license_en_xps;
            try
            {
                var currentDomain = AppDomain.CurrentDomain.BaseDirectory;
                var xpsFile = Path.Combine(currentDomain, $@"..\LicenseDocs\{filename}");

                XpsDocument document = new XpsDocument(xpsFile, FileAccess.Read);
                FixedDocumentSequence = document.GetFixedDocumentSequence();
            }
            catch (Exception e )
            {
                MessageBox.Show("License file not found!" + e.Message, "Error", MessageBoxButton.OK);
            }
        }
    }
}
